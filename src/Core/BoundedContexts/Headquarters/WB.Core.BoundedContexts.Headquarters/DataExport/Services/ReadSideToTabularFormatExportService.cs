﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.DataExport.Factories;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Resources;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.ValueObjects.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using WB.Core.SharedKernels.SurveySolutions.Implementation.ServiceVariables;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.Services
{
    internal class ReadSideToTabularFormatExportService : ITabularFormatExportService
    {
        private const double InterviewsExportProgressModifier = 0.8;
        private const double CommentsExportProgressModifier = 0.8;
        private readonly string commentsFileName = "interview_comments";
        private readonly string interviewActionsFileName = "interview_actions";
        private readonly string[] actionFileColumns = new[] { "InterviewId", "Action", "Originator", "Role", "Date", "Time" };
        private readonly Regex removeNewLineRegEx = new Regex(@"\t|\n|\r");

        private readonly string dataFileExtension = "tab";

        private readonly string separator;
        private readonly int returnRecordLimit;

        private readonly ITransactionManagerProvider transactionManagerProvider;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ICsvWriterFactory csvWriterFactory;
        private readonly ISerializer serializer;

        private readonly IQueryableReadSideRepositoryReader<InterviewStatuses> interviewActionsDataStorage;
        private readonly IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage;
        private readonly IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage;

        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;
        private readonly IReadSideKeyValueStorage<InterviewData> interviewDatas;
        private readonly IExportViewFactory exportViewFactory;
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage;
        private readonly ITransactionManager transactionManager;

        public ReadSideToTabularFormatExportService(
            ITransactionManagerProvider transactionManagerProvider,
            IFileSystemAccessor fileSystemAccessor,
            ICsvWriterFactory csvWriterFactory,
            ISerializer serializer,
            InterviewDataExportSettings interviewDataExportSettings,
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewActionsDataStorage,
            IQueryableReadSideRepositoryReader<InterviewCommentaries> interviewCommentariesStorage,
            IReadSideKeyValueStorage<QuestionnaireExportStructure> questionnaireExportStructureStorage, 
            IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries, 
            IReadSideKeyValueStorage<InterviewData> interviewDatas, 
            IExportViewFactory exportViewFactory, 
            ITransactionManager transactionManager, 
            IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireDocumentVersionedStorage)
        {
            this.transactionManagerProvider = transactionManagerProvider;
            this.fileSystemAccessor = fileSystemAccessor;
            this.csvWriterFactory = csvWriterFactory;
            this.interviewActionsDataStorage = interviewActionsDataStorage;
            this.interviewCommentariesStorage = interviewCommentariesStorage;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.interviewSummaries = interviewSummaries;
            this.interviewDatas = interviewDatas;
            this.exportViewFactory = exportViewFactory;
            this.transactionManager = transactionManager;
            this.questionnaireDocumentVersionedStorage = questionnaireDocumentVersionedStorage;
            this.serializer = serializer;

            this.separator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            this.returnRecordLimit = interviewDataExportSettings.MaxRecordsCountPerOneExportQuery;
        }

        public void ExportInterviewsInTabularFormatAsync(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress)
        {
            QuestionnaireExportStructure questionnaireExportStructure = this.BuildQuestionnaireExportStructure(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);

            List<Guid> interviewIdsToExport =
                this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ =>
                            _.Where(x => x.QuestionnaireId == questionnaireIdentity.QuestionnaireId && x.QuestionnaireVersion == questionnaireIdentity.Version)
                                .OrderBy(x => x.InterviewId)
                                .Select(x => x.InterviewId).ToList()));

            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
            this.ExportInterviews(interviewIdsToExport, basePath, questionnaireExportStructure, progress);

            Expression<Func<InterviewStatuses, bool>> whereClauseForAction =
                interviewComments =>
                    interviewComments.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                    interviewComments.QuestionnaireVersion == questionnaireIdentity.Version;

            this.ExportComments(questionnaireExportStructure, false, basePath, progress);
            this.ExportActionsInTabularFormatAsync(whereClauseForAction, basePath, progress);
        }

        private void ExportInterviews(List<Guid> interviewIdsToExport, 
            string basePath, 
            QuestionnaireExportStructure questionnaireExportStructure, 
            IProgress<int> progress)
        {
            int totalInterviewsProcessed = 0;
            foreach (var interviewId in interviewIdsToExport)
            {
                var interviewData = this.transactionManager.ExecuteInQueryTransaction(() => this.interviewDatas.GetById(interviewId));

                InterviewDataExportView interviewExportStructure =
                    this.exportViewFactory.CreateInterviewDataExportView(questionnaireExportStructure,
                        interviewData); 

                InterviewExportedDataRecord exportedData = this.CreateInterviewExportedData(
                    interviewExportStructure, questionnaireExportStructure.QuestionnaireId, questionnaireExportStructure.Version);

                var result = new Dictionary<string, List<string[]>>();
                Dictionary<string, string[]> deserializedExportedData =
                    this.serializer.Deserialize<Dictionary<string, string[]>>(exportedData.Data);
                foreach (var levelName in deserializedExportedData.Keys)
                {
                    foreach (var dataByLevel in deserializedExportedData[levelName])
                    {
                        if (!result.ContainsKey(levelName))
                        {
                            result.Add(levelName, new List<string[]>());
                        }

                        result[levelName].Add(dataByLevel.Split(ExportFileSettings.SeparatorOfExportedDataFile));
                    }
                }

                foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
                {
                    var dataByTheLevelFilePath = this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(level.LevelName, this.dataFileExtension));

                    if (result.ContainsKey(level.LevelName))
                    {
                        this.WriteData(dataByTheLevelFilePath, result[level.LevelName]);
                    }
                }

                totalInterviewsProcessed++;
                progress.Report((int) (totalInterviewsProcessed.PercentOf(interviewIdsToExport.Count) * InterviewsExportProgressModifier));
            }
        }

        public void ExportApprovedInterviewsInTabularFormatAsync(QuestionnaireIdentity questionnaireIdentity, string basePath, IProgress<int> progress)
        {
            QuestionnaireExportStructure questionnaireExportStructure = this.BuildQuestionnaireExportStructure(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version);
            List<Guid> interviewIdsToExport =
                this.transactionManager.ExecuteInQueryTransaction(() =>
                    this.interviewSummaries.Query(_ => _.Where(x => x.QuestionnaireId == questionnaireIdentity.QuestionnaireId && x.QuestionnaireVersion == questionnaireIdentity.Version)
                                                .Where(x => x.Status == InterviewStatus.ApprovedByHeadquarters)
                                                .OrderBy(x => x.InterviewId)
                                                .Select(x => x.InterviewId).ToList()));

            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
            this.ExportInterviews(interviewIdsToExport, basePath, questionnaireExportStructure, progress);

            Expression<Func<InterviewStatuses, bool>> whereClauseForAction =
                interviewWithStatusHistory =>
                    interviewWithStatusHistory.QuestionnaireId == questionnaireIdentity.QuestionnaireId &&
                    interviewWithStatusHistory.QuestionnaireVersion == questionnaireIdentity.Version &&
                    interviewWithStatusHistory.InterviewCommentedStatuses.Select(s => s.Status)
                        .Any(s => s == InterviewExportedAction.ApprovedByHeadquarter);

            this.ExportComments(questionnaireExportStructure, true, basePath, progress);
            this.ExportActionsInTabularFormatAsync(whereClauseForAction, basePath, progress);
        }

        public void CreateHeaderStructureForPreloadingForQuestionnaire(QuestionnaireIdentity questionnaireIdentity, string basePath)
        {
            QuestionnaireExportStructure questionnaireExportStructure =
              this.transactionManagerProvider.GetTransactionManager()
                  .ExecuteInQueryTransaction(
                      () =>
                          this.questionnaireExportStructureStorage.AsVersioned()
                              .Get(questionnaireIdentity.QuestionnaireId.FormatGuid(), questionnaireIdentity.Version));

            if (questionnaireExportStructure == null)
                return;

            this.CreateDataSchemaForInterviewsInTabular(questionnaireExportStructure, basePath);
        }

        public string[] GetTabularDataFilesFromFolder(string basePath)
        {
            var filesInDirectory = this.fileSystemAccessor.GetFilesInDirectory(basePath).Where(fileName => fileName.EndsWith("." + this.dataFileExtension)).ToArray();
            return filesInDirectory;
        }

        private void ExportComments(QuestionnaireExportStructure questionnaireExportStructure, 
            bool exportApprovedOnly, 
            string basePath, 
            IProgress<int> progress)
        {
            string commentsFilePath =
                this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(this.commentsFileName, this.dataFileExtension));

            int maxRosterDepthInQuestionnaire =
                questionnaireExportStructure.HeaderToLevelMap.Values.Max(x => x.LevelScopeVector.Count);

            bool hasAtLeastOneRoster =
                questionnaireExportStructure.HeaderToLevelMap.Values.Any(x => x.LevelScopeVector.Count > 0);

            var commentsHeader = new List<string> { "Order", "Originator", "Role", "Date", "Time", "Variable" };

            if (hasAtLeastOneRoster)
                commentsHeader.Add("Roster");

            commentsHeader.Add("InterviewId");

            for (int i = 1; i <= maxRosterDepthInQuestionnaire; i++)
            {
                commentsHeader.Add($"Id{i}");
            }
            commentsHeader.Add("Comment");

            this.WriteData(commentsFilePath, new[] { commentsHeader.ToArray() });

            Expression<Func<InterviewCommentaries, bool>> whereClauseForComments;
            if (exportApprovedOnly)
            {
                whereClauseForComments =
                    interviewComments =>
                        interviewComments.QuestionnaireId == questionnaireExportStructure.QuestionnaireId.FormatGuid() &&
                        interviewComments.QuestionnaireVersion == questionnaireExportStructure.Version;
            }
            else
            {
                whereClauseForComments =
                    interviewComments =>
                        interviewComments.QuestionnaireId == questionnaireExportStructure.QuestionnaireId.FormatGuid() &&
                        interviewComments.QuestionnaireVersion == questionnaireExportStructure.Version &&
                        interviewComments.IsApprovedByHQ;
            }


            var countOfAllRecords =
              this.transactionManagerProvider.GetTransactionManager()
                  .ExecuteInQueryTransaction(() => this.interviewCommentariesStorage.Query(_ => _.Where(whereClauseForComments).SelectMany(x => x.Commentaries).Count()));

            int skip = 0;

            while (skip < countOfAllRecords)
            {
                var skipAtCurrentIteration = skip;

                string[][] exportComments = this.transactionManagerProvider.GetTransactionManager()
                                                            .ExecuteInQueryTransaction(
                                                                () => this.QueryCommentsChunkFromReadSide(whereClauseForComments, skipAtCurrentIteration, maxRosterDepthInQuestionnaire, hasAtLeastOneRoster));

                this.WriteData(commentsFilePath, exportComments);
                skip = skip + this.returnRecordLimit;

                progress.Report((int)(0.8 + skip.PercentOf(countOfAllRecords) * CommentsExportProgressModifier));
            }

            progress.Report(90);
        }

        private void ExportActionsInTabularFormatAsync(Expression<Func<InterviewStatuses, bool>> whereClauseForAction, 
            string basePath, 
            IProgress<int> progress)
        {
            var actionFilePath = this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(this.interviewActionsFileName, this.dataFileExtension));

            this.WriteData(actionFilePath, new[] { this.actionFileColumns });

            foreach (var queryActionsChunk in this.GetTasksForQueryActionsByChunks(whereClauseForAction))
            {
                this.WriteData(actionFilePath, queryActionsChunk);
            }
            progress.Report(100);
        }

        private void CreateDataSchemaForInterviewsInTabular(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            foreach (var level in questionnaireExportStructure.HeaderToLevelMap.Values)
            {
                var dataByTheLevelFilePath =
                    this.fileSystemAccessor.CombinePath(basePath, Path.ChangeExtension(level.LevelName, this.dataFileExtension));

                var interviewLevelHeader = new List<string> {level.LevelIdColumnName};


                if (level.IsTextListScope)
                {
                    interviewLevelHeader.AddRange(level.ReferencedNames);
                }

                foreach (ExportedHeaderItem question in level.HeaderItems.Values)
                {
                    interviewLevelHeader.AddRange(question.ColumnNames);
                }

                if (level.LevelScopeVector.Length == 0)
                {
                    interviewLevelHeader.AddRange(ServiceColumns.SystemVariables.Select(systemVariable => systemVariable.VariableExportColumnName));
                }

                for (int i = 0; i < level.LevelScopeVector.Length; i++)
                {
                    interviewLevelHeader.Add($"{ServiceColumns.ParentId}{i + 1}");
                }

                this.WriteData(dataByTheLevelFilePath, new[] { interviewLevelHeader.ToArray() });
            }
        }

        private QuestionnaireExportStructure BuildQuestionnaireExportStructure(Guid questionnaireId, long questionnaireVersion)
        {
            QuestionnaireDocumentVersioned questionnaire =
                this.transactionManager.ExecuteInQueryTransaction(() => 
                    this.questionnaireDocumentVersionedStorage.AsVersioned()
                        .Get(questionnaireId.FormatGuid(), questionnaireVersion));
            var questionnaireExportStructure =
                this.exportViewFactory.CreateQuestionnaireExportStructure(questionnaire.Questionnaire, questionnaireVersion);
            return questionnaireExportStructure;
        }

        private IEnumerable<string[][]> GetTasksForQueryActionsByChunks(Expression<Func<InterviewStatuses, bool>> whereClauseForAction)
        {
            return this.QueryByChunks(
                skip => this.QueryActionsChunkFromReadSide(whereClauseForAction, skip),
                        () => this.interviewActionsDataStorage.Query(
                            _ => _.Where(whereClauseForAction).SelectMany(x => x.InterviewCommentedStatuses).Count()));
        }

        private string[][] QueryCommentsChunkFromReadSide(Expression<Func<InterviewCommentaries, bool>> queryComments, int skip, int maxRosterDepthInQuestionnaire, bool hasAtLeastOneRoster)
        {
            var comments = this.interviewCommentariesStorage.Query(
                            _ =>
                                _.Where(queryComments)
                                    .SelectMany(
                                        interviewComments => interviewComments.Commentaries,
                                        (interview, comment) => new { interview.InterviewId, Comments = comment })
                                    .Select(
                                        i =>
                                            new
                                            {
                                                i.InterviewId,
                                                i.Comments.CommentSequence,
                                                i.Comments.OriginatorName,
                                                i.Comments.OriginatorRole,
                                                i.Comments.Timestamp,
                                                i.Comments.Variable,
                                                i.Comments.Roster,
                                                i.Comments.RosterVector,
                                                i.Comments.Comment
                                            }).OrderBy(i => i.Timestamp).Skip(skip).Take(this.returnRecordLimit).ToList());

            var result = new List<string[]>();

            foreach (var interview in comments)
            {
                var resultRow = new List<string>
                {
                    interview.CommentSequence.ToString(),
                    interview.OriginatorName,
                    this.GetUserRole(interview.OriginatorRole),
                    interview.Timestamp.ToString("d", CultureInfo.InvariantCulture),
                    interview.Timestamp.ToString("T", CultureInfo.InvariantCulture),
                    interview.Variable
                };

                if (hasAtLeastOneRoster)
                    resultRow.Add(interview.Roster);

                resultRow.Add(interview.InterviewId);

                for (int i = 0; i < maxRosterDepthInQuestionnaire; i++)
                {
                    resultRow.Add(interview.RosterVector.Length > i ? interview.RosterVector[i].ToString(CultureInfo.InvariantCulture) : "");
                }

                resultRow.Add(interview.Comment);

                result.Add(resultRow.ToArray());
            }
            return result.ToArray();
        }

        private string[][] QueryActionsChunkFromReadSide(Expression<Func<InterviewStatuses, bool>> queryActions, int skip)
        {
            var interviews =
              this.interviewActionsDataStorage.Query(
                  _ =>
                      _.Where(queryActions)
                          .SelectMany(
                              interviewWithStatusHistory => interviewWithStatusHistory.InterviewCommentedStatuses,
                              (interview, status) => new { interview.InterviewId, StatusHistory = status })
                          .Select(
                              i =>
                                  new
                                  {
                                      i.InterviewId,
                                      i.StatusHistory.Status,
                                      i.StatusHistory.StatusChangeOriginatorName,
                                      i.StatusHistory.StatusChangeOriginatorRole,
                                      i.StatusHistory.Timestamp

                                  }).OrderBy(i => i.Timestamp).Skip(skip).Take(this.returnRecordLimit).ToList());

            var result = new List<string[]>();

            foreach (var interview in interviews)
            {
                var resultRow = new List<string>
                {
                    interview.InterviewId,
                    interview.Status.ToString(),
                    interview.StatusChangeOriginatorName,
                    this.GetUserRole(interview.StatusChangeOriginatorRole),
                    interview.Timestamp.ToString("d", CultureInfo.InvariantCulture),
                    interview.Timestamp.ToString("T", CultureInfo.InvariantCulture)
                };
                result.Add(resultRow.ToArray());
            }
            return result.ToArray();
        }

        private IEnumerable<T> QueryByChunks<T>(Func<int, T> dataQuery, Func<int> countQuery)
        {
            var countOfAllRecords =
                this.transactionManagerProvider.GetTransactionManager()
                    .ExecuteInQueryTransaction(countQuery);

            int skip = 0;

            while (skip < countOfAllRecords)
            {
                var skipAtCurrentIteration = skip;

                yield return this.transactionManagerProvider.GetTransactionManager()
                                                            .ExecuteInQueryTransaction(() => dataQuery(skipAtCurrentIteration));

                skip = skip + this.returnRecordLimit;
            }
        }

        private string GetUserRole(UserRoles userRole)
        {
            switch (userRole)
            {
                case UserRoles.Operator:
                    return FileBasedDataExportRepositoryWriterMessages.Interviewer;
                case UserRoles.Supervisor:
                    return FileBasedDataExportRepositoryWriterMessages.Supervisor;
                case UserRoles.Headquarter:
                    return FileBasedDataExportRepositoryWriterMessages.Headquarter;
                case UserRoles.Administrator:
                    return FileBasedDataExportRepositoryWriterMessages.Administrator;
            }
            return FileBasedDataExportRepositoryWriterMessages.UnknownRole;
        }

        private void WriteData(string filePath, IEnumerable<string[]> records)
        {
            using (var fileStream = this.fileSystemAccessor.OpenOrCreateFile(filePath, true))
            using (var tabWriter = this.csvWriterFactory.OpenCsvWriter(fileStream, this.separator))
            {
                foreach (var dataRow in records)
                {
                    foreach (var cell in dataRow)
                    {
                        var valueToWrite = string.IsNullOrEmpty(cell) ? "" : this.removeNewLineRegEx.Replace(cell, "");
                        tabWriter.WriteField(valueToWrite);
                    }

                    tabWriter.NextRecord();
                }
            }
        }

        private InterviewExportedDataRecord CreateInterviewExportedData(InterviewDataExportView interviewDataExportView, Guid questionnaireId, long questionnaireVersion)
        {
            var interviewData = new Dictionary<string, string[]>();

            var stringSeparator = ExportFileSettings.SeparatorOfExportedDataFile.ToString();
            foreach (var interviewDataExportLevelView in interviewDataExportView.Levels)
            {
                var recordsByLevel = new List<string>();
                foreach (var interviewDataExportRecord in interviewDataExportLevelView.Records)
                {
                    var parametersToConcatenate = new List<string> { interviewDataExportRecord.RecordId };

                    parametersToConcatenate.AddRange(interviewDataExportRecord.ReferenceValues);

                    foreach (var exportedQuestion in interviewDataExportRecord.Questions)
                    {
                        parametersToConcatenate.AddRange(exportedQuestion.Answers.Select(itemValue => string.IsNullOrEmpty(itemValue) ? "" : itemValue));
                    }

                    parametersToConcatenate.AddRange(interviewDataExportRecord.SystemVariableValues);
                    parametersToConcatenate.AddRange(interviewDataExportRecord.ParentRecordIds);

                    recordsByLevel.Add(string.Join(stringSeparator,
                            parametersToConcatenate.Select(v => v.Replace(stringSeparator, ""))));
                }
                interviewData.Add(interviewDataExportLevelView.LevelName, recordsByLevel.ToArray());
            }
            var interviewExportedData = new InterviewExportedDataRecord
            {
                InterviewId = interviewDataExportView.InterviewId.FormatGuid(),
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = questionnaireVersion,
                Data = this.serializer.SerializeToByteArray(interviewData),
            };

            return interviewExportedData;
        }
    }
}