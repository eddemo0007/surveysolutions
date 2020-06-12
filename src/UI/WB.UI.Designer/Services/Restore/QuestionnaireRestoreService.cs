﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zip;
using Main.Core.Documents;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Extensions;

namespace WB.UI.Designer.Services.Restore
{
    class QuestionnaireRestoreService : IQuestionnaireRestoreService
    {
        private readonly ILogger<QuestionnaireRestoreService> logger;
        private readonly ISerializer serializer;
        private readonly ICommandService commandService;
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationsService;
        private readonly DesignerDbContext dbContext;
        private readonly ICategoriesService categoriesService;

        public QuestionnaireRestoreService(ILogger<QuestionnaireRestoreService> logger, ISerializer serializer, ICommandService commandService, ILookupTableService lookupTableService, IAttachmentService attachmentService, ITranslationsService translationsService, DesignerDbContext dbContext, ICategoriesService categoriesService)
        {
            this.logger = logger;
            this.serializer = serializer;
            this.commandService = commandService;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationsService = translationsService;
            this.dbContext = dbContext;
            this.categoriesService = categoriesService;
        }

        public void RestoreQuestionnaire(Stream archive, Guid responsibleId, RestoreState state)
        {
            using var zipStream = new ZipInputStream(archive);
            
            ZipEntry zipEntry = zipStream.GetNextEntry();

            while (zipEntry != null)
            {
                this.RestoreDataFromZipFileEntry(zipEntry, zipStream, responsibleId, state);

                zipEntry = zipStream.GetNextEntry();
            }

            state.Error = "";
            foreach (Guid attachmentId in state.GetPendingAttachments())
            {
                state.Error += $"Attachment '{attachmentId.FormatGuid()}' was not restored because there are not enough data for it in it's folder." + Environment.NewLine;
            }
        }

        public void RestoreDataFromZipFileEntry(ZipEntry zipEntry, ZipInputStream zipStream, Guid responsibleId, RestoreState state)
        {
            try
            {
                string[] zipEntryPathChunks = zipEntry.FileName.Split(
                    new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                    StringSplitOptions.RemoveEmptyEntries);

                bool isInsideQuestionnaireFolder = Guid.TryParse(
                    zipEntryPathChunks[0].Split(new[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries).Last(),
                    out var questionnaireId);

                if (!isInsideQuestionnaireFolder)
                {
                    return;
                }

                bool isQuestionnaireDocumentEntry =
                    zipEntryPathChunks.Length == 2 &&
                    zipEntryPathChunks[1].ToLower().EndsWith(".json");

                bool isAttachmentEntry =
                    zipEntryPathChunks.Length == 4 &&
                    string.Equals(zipEntryPathChunks[1], "attachments", StringComparison.CurrentCultureIgnoreCase);

                bool isLookupTableEntry =
                    zipEntryPathChunks.Length == 3 &&
                    string.Equals(zipEntryPathChunks[1], "lookup tables", StringComparison.CurrentCultureIgnoreCase) &&
                    zipEntryPathChunks[2].ToLower().EndsWith(".txt");

                bool isTranslationEntry =
                    zipEntryPathChunks.Length == 3 &&
                    string.Equals(zipEntryPathChunks[1], "translations", StringComparison.CurrentCultureIgnoreCase) &&
                    (".xlsx".Equals(Path.GetExtension(zipEntryPathChunks[2]), StringComparison.InvariantCultureIgnoreCase) ||
                     ".ods".Equals(Path.GetExtension(zipEntryPathChunks[2]), StringComparison.InvariantCultureIgnoreCase));

                bool isCollectionsEntry =
                    zipEntryPathChunks.Length == 3 &&
                    string.Equals(zipEntryPathChunks[1], "categories", StringComparison.CurrentCultureIgnoreCase) &&
                    (".xlsx".Equals(Path.GetExtension(zipEntryPathChunks[2]), StringComparison.InvariantCultureIgnoreCase) ||
                     ".ods".Equals(Path.GetExtension(zipEntryPathChunks[2]), StringComparison.InvariantCultureIgnoreCase));

                if (isQuestionnaireDocumentEntry)
                {
                    string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                    var questionnaireDocument = this.serializer.Deserialize<QuestionnaireDocument>(textContent);
                    questionnaireDocument.PublicKey = questionnaireId;

                    var command = new ImportQuestionnaire(responsibleId, questionnaireDocument);
                    this.commandService.Execute(command);

                    this.translationsService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);
                    this.categoriesService.DeleteAllByQuestionnaireId(questionnaireDocument.PublicKey);

                    state.Success.AppendLine($"[{zipEntry.FileName}]");
                    state.Success.AppendLine($"    Restored questionnaire document '{questionnaireDocument.Title}' with id '{questionnaireDocument.PublicKey.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                else if (isAttachmentEntry)
                {
                    var attachmentId = Guid.Parse(zipEntryPathChunks[2]);
                    var fileName = zipEntryPathChunks[3];

                    if (string.Equals(fileName, "content-type.txt", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                        state.StoreAttachmentContentType(attachmentId, textContent);
                        state.Success.AppendLine($"[{zipEntry.FileName}]");
                        state.Success.AppendLine($"    Found content-type '{textContent}' for attachment '{attachmentId.FormatGuid()}'.");
                    }
                    else
                    {
                        byte[] binaryContent = zipStream.ReadToEnd();

                        state.StoreAttachmentFile(attachmentId, fileName, binaryContent);
                        state.Success.AppendLine($"[{zipEntry.FileName}]");
                        state.Success.AppendLine($"    Found file data '{fileName}' for attachment '{attachmentId.FormatGuid()}'.");
                    }

                    var attachment = state.GetAttachment(attachmentId);

                    if (attachment.HasAllDataForRestore())
                    {
                        string attachmentContentId = this.attachmentService.CreateAttachmentContentId(attachment.BinaryContent!);

                        this.attachmentService.SaveContent(attachmentContentId, attachment.ContentType!, attachment.BinaryContent!);
                        this.attachmentService.SaveMeta(attachmentId, questionnaireId, attachmentContentId, attachment.FileName!);

                        state.RemoveAttachment(attachmentId);

                        state.Success.AppendLine($"    Restored attachment '{attachmentId.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}' using file '{attachment.FileName}' and content-type '{attachment.ContentType}'.");
                        state.RestoredEntitiesCount++;
                    }
                }
                else if (isLookupTableEntry)
                {
                    var lookupTableId = Guid.Parse(Path.GetFileNameWithoutExtension(zipEntryPathChunks[2]));

                    string textContent = new StreamReader(zipStream, Encoding.UTF8).ReadToEnd();

                    this.lookupTableService.SaveLookupTableContent(questionnaireId, lookupTableId, textContent);

                    state.Success.AppendLine($"[{zipEntry.FileName}].");
                    state.Success.AppendLine($"    Restored lookup table '{lookupTableId.FormatGuid()}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                else if (isTranslationEntry)
                {
                    var translationIdString = Path.GetFileNameWithoutExtension(zipEntryPathChunks[2]);
                    byte[]? excelContent;

                    using (var memoryStream = new MemoryStream())
                    {
                        zipStream.CopyTo(memoryStream);
                        excelContent = memoryStream.ToArray();
                    }

                    var translationId = Guid.Parse(translationIdString);
                    this.translationsService.Store(questionnaireId, translationId, excelContent);

                    state.Success.AppendLine($"[{zipEntry.FileName}].");
                    state.Success.AppendLine($"    Restored translation '{translationId}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                else if (isCollectionsEntry)
                {
                    var collectionsIdString = Path.GetFileNameWithoutExtension(zipEntryPathChunks[2]);
                    var collectionsId = Guid.Parse(collectionsIdString);

                    this.categoriesService.Store(questionnaireId, collectionsId, zipStream, CategoriesFileType.Excel);

                    state.Success.AppendLine($"[{zipEntry.FileName}].");
                    state.Success.AppendLine($"    Restored categories '{collectionsId}' for questionnaire '{questionnaireId.FormatGuid()}'.");
                    state.RestoredEntitiesCount++;
                }
                else
                {
                }

                dbContext.SaveChanges();
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, $"Error processing zip file entry '{zipEntry.FileName}' during questionnaire restore from backup.");
                state.Error = $"Error processing zip file entry '{zipEntry.FileName}'.{Environment.NewLine}{exception}";
                logger.LogError(state.Error);
            }
        }
    }
}