﻿using System;
using System.Linq;
using Main.Core.Documents;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    public class QuestionnaireHistoryVersionsService : IQuestionnaireHistoryVersionsService
    {
        private readonly DesignerDbContext dbContext;
        private readonly IEntitySerializer<QuestionnaireDocument> entitySerializer;
        private readonly IOptions<QuestionnaireHistorySettings> historySettings;
        private readonly IPatchApplier patchApplier;
        private readonly IPatchGenerator patchGenerator;

        public QuestionnaireHistoryVersionsService(DesignerDbContext dbContext,
            IEntitySerializer<QuestionnaireDocument> entitySerializer,
            IOptions<QuestionnaireHistorySettings> historySettings,
            IPatchApplier patchApplier,
            IPatchGenerator patchGenerator)
        {
            this.dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            this.entitySerializer = entitySerializer;
            this.historySettings = historySettings;
            this.patchApplier = patchApplier;
            this.patchGenerator = patchGenerator;
        }

        public QuestionnaireDocument GetByHistoryVersion(Guid historyReferenceId)
        {
            var questionnaireChangeRecord = this.dbContext.QuestionnaireChangeRecords.Find(historyReferenceId.FormatGuid());
            if (questionnaireChangeRecord == null)
                return null;

            if (questionnaireChangeRecord.ResultingQuestionnaireDocument != null)
            {
                var resultingQuestionnaireDocument = questionnaireChangeRecord.ResultingQuestionnaireDocument;
                var questionnaireDocument = this.entitySerializer.Deserialize(resultingQuestionnaireDocument);
                return questionnaireDocument;
            }

            var history = (from h in this.dbContext.QuestionnaireChangeRecords
                           where h.Sequence >= questionnaireChangeRecord.Sequence
                              && h.QuestionnaireId == questionnaireChangeRecord.QuestionnaireId &&
                              (h.Patch != null || h.ResultingQuestionnaireDocument != null)
                        orderby h.Sequence descending 
                        select new
                        {
                            h.Sequence,
                            h.ResultingQuestionnaireDocument,
                            DiffWithPreviousVersion = h.Patch
                        }).ToList();

            var questionnaire = history.First().ResultingQuestionnaireDocument;
            foreach (var patch in history.Skip(1))
            {
                questionnaire = this.patchApplier.Apply(questionnaire, patch.DiffWithPreviousVersion);
            }

            return entitySerializer.Deserialize(questionnaire);
        }

        public void RemoveOldQuestionnaireHistory(string sQuestionnaireId, int? maxSequenceByQuestionnaire, int maxHistoryDepth)
        {
            var minSequence = (maxSequenceByQuestionnaire ?? 0) -
                              maxHistoryDepth + 2;
            if (minSequence < 0) return;

            var oldChangeRecord = this.dbContext.QuestionnaireChangeRecords
                .Where(x => x.QuestionnaireId == sQuestionnaireId && x.Sequence < minSequence
                                                                  && (x.ResultingQuestionnaireDocument != null || x.Patch != null))
                .OrderBy(x => x.Sequence)
                .ToList();

            foreach (var questionnaireChangeRecord in oldChangeRecord)
            {
                questionnaireChangeRecord.Patch = null;
                questionnaireChangeRecord.ResultingQuestionnaireDocument = null;
            }
        }

        public void AddQuestionnaireChangeItem(
            Guid questionnaireId,
            Guid responsibleId,
            string userName,
            QuestionnaireActionType actionType,
            QuestionnaireItemType targetType,
            Guid targetId,
            string targetTitle,
            string targetNewTitle,
            int? affectedEntries,
            DateTime? targetDateTime,
            QuestionnaireDocument questionnaireDocument,
            QuestionnaireChangeReference reference = null)
        {
            var sQuestionnaireId = questionnaireId.FormatGuid();

            var maxSequenceByQuestionnaire = this.dbContext.QuestionnaireChangeRecords
                .Where(y => y.QuestionnaireId == sQuestionnaireId).Select(y => (int?) y.Sequence).Max();

            var previousChange = (from h in this.dbContext.QuestionnaireChangeRecords
                                 where h.QuestionnaireId == sQuestionnaireId && h.ResultingQuestionnaireDocument != null
                                orderby h.Sequence descending 
                                select h
                                    ).FirstOrDefault();

            if (previousChange != null && questionnaireDocument != null)
            {
                var previousVersion = previousChange.ResultingQuestionnaireDocument;
                var left = this.entitySerializer.Serialize(questionnaireDocument);
                var right = previousVersion;

                var patch = this.patchGenerator.Diff(left, right);
                previousChange.ResultingQuestionnaireDocument = null;
                previousChange.Patch = patch;
            }

            var questionnaireChangeItem = new QuestionnaireChangeRecord
            {
                QuestionnaireChangeRecordId = Guid.NewGuid().FormatGuid(),
                QuestionnaireId = questionnaireId.FormatGuid(),
                UserId = responsibleId,
                UserName = userName,
                Timestamp = DateTime.UtcNow,
                Sequence = maxSequenceByQuestionnaire + 1 ?? 0,
                ActionType = actionType,
                TargetItemId = targetId,
                TargetItemTitle = targetTitle,
                TargetItemType = targetType,
                TargetItemNewTitle = targetNewTitle,
                AffectedEntriesCount = affectedEntries,
                TargetItemDateTime = targetDateTime,
            };

            if (reference != null)
            {
                reference.QuestionnaireChangeRecord = questionnaireChangeItem;
                questionnaireChangeItem.References.Add(reference);
            }

            if (questionnaireDocument != null)
            {
                questionnaireChangeItem.ResultingQuestionnaireDocument = this.entitySerializer.Serialize(questionnaireDocument);
            }

            this.dbContext.QuestionnaireChangeRecords.Add(questionnaireChangeItem);

            this.RemoveOldQuestionnaireHistory(sQuestionnaireId, 
                maxSequenceByQuestionnaire, 
                historySettings.Value.QuestionnaireChangeHistoryLimit);
            this.dbContext.SaveChanges();
        }

        public string GetDiffWithLastStoredVersion(QuestionnaireDocument questionnaire)
        {
            var previousVersion = this.GetLastStoredQuestionnaireVersion(questionnaire);
            var left = this.entitySerializer.Serialize(previousVersion);
            var right = this.entitySerializer.Serialize(questionnaire);

            var patch = this.patchGenerator.Diff(left, right);
            return patch;
        }

        private QuestionnaireDocument GetLastStoredQuestionnaireVersion(QuestionnaireDocument questionnaireDocument)
        {
            if (questionnaireDocument == null)
                return null;

            var existingSnapshot =  
                (from h in this.dbContext.QuestionnaireChangeRecords
                 where h.QuestionnaireId == questionnaireDocument.Id
                orderby h.Sequence 
                select h.QuestionnaireChangeRecordId).FirstOrDefault();

            if (existingSnapshot == null)
                return null;

            var resultingQuestionnaireDocument = this.GetByHistoryVersion(Guid.Parse(existingSnapshot));

            return resultingQuestionnaireDocument;
        }
    }
}
