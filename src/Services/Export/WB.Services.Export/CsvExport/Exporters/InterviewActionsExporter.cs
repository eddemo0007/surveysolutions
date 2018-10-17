﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;
using WB.Services.Export.Utils;
using WB.Services.Infrastructure.Tenant;

namespace WB.Services.Export.CsvExport.Exporters
{
    public class InterviewActionsExporter: IInterviewActionsExporter
    {
        private readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;
        public string InterviewActionsFileName => "interview__actions";

        public DoExportFileHeader[] ActionFileColumns => new []
        {
            new DoExportFileHeader("interview__id", "Unique 32-character long identifier of the interview"),
            new DoExportFileHeader("interview__key", "Identifier of the interview"),
            new DoExportFileHeader("Action", "Type of action taken"),
            new DoExportFileHeader("Originator", "Login name of the person performing the action"),
            new DoExportFileHeader("Role", "System role of the person performing the action"),
            new DoExportFileHeader("ResponsibleName", "Login name of the person now responsible for the interview"),
            new DoExportFileHeader("ResponsibleRole", "System role of the person now responsible for the interview"),
            new DoExportFileHeader("Date", "Date when the action was taken"),
            new DoExportFileHeader("Time", "Time when the action was taken")
        };

        private readonly string dataFileExtension = "tab";
        private readonly ICsvWriter csvWriter;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;

        public InterviewActionsExporter(IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            ICsvWriter csvWriter,
            ITenantApi<IHeadquartersApi> tenantApi)
        {
            this.interviewDataExportSettings = interviewDataExportSettings;
            this.csvWriter = csvWriter;
            this.tenantApi = tenantApi;
        }

        public async Task ExportAsync(TenantInfo tenant, QuestionnaireId questionnaireIdentity, List<Guid> interviewIdsToExport,
            string basePath, IProgress<int> progress)
        {
            var actionFilePath = Path.Combine(basePath, Path.ChangeExtension(this.InterviewActionsFileName, this.dataFileExtension));
            var batchSize = this.interviewDataExportSettings.Value.MaxRecordsCountPerOneExportQuery;

            var fileColumns = ActionFileColumns.Select(a => a.Title).ToArray();
            this.csvWriter.WriteData(actionFilePath, new[] { fileColumns }, ExportFileSettings.DataFileSeparator.ToString());

            long totalProcessedCount = 0;
            var api = this.tenantApi.For(tenant);

            foreach (var interviewsBatch in interviewIdsToExport.Batch(batchSize))
            {
                var interviewIdsStrings = interviewsBatch.ToArray();
                var actionsChunk = await this.QueryActionsChunkFromReadSide(api, interviewIdsStrings);

                this.csvWriter.WriteData(actionFilePath, actionsChunk, ExportFileSettings.DataFileSeparator.ToString());

                totalProcessedCount += interviewIdsStrings.Length;
                progress.Report(totalProcessedCount.PercentOf(interviewIdsToExport.Count));
            }

            progress.Report(100);
        }

        public void ExportDoFile(QuestionnaireExportStructure questionnaireExportStructure, string basePath)
        {
            var doContent = new DoFile();

            doContent.BuildInsheet(Path.ChangeExtension(this.InterviewActionsFileName, this.dataFileExtension));
            doContent.AppendLine();

            foreach (var actionFileColumn in ActionFileColumns)
            {
                doContent.AppendLabelToVariableMatching(actionFileColumn.Title, actionFileColumn.Description);
            }

            var fileName = $"{InterviewActionsFileName}.{DoFile.ContentFileNameExtension}";
            var contentFilePath = Path.Combine(basePath, fileName);

            File.WriteAllText(contentFilePath, doContent.ToString());
        }

        private async Task<List<string[]>> QueryActionsChunkFromReadSide(IHeadquartersApi api, Guid[] interviewIds)
        {
            var interviews = await api.GetInterviewSummariesBatchAsync(interviewIds);

            var result = new List<string[]>();

            foreach (var interview in interviews)
            {
                var resultRow = new List<string>
                {
                    interview.InterviewId.FormatGuid(),
                    interview.Key,
                    interview.Status.ToString(),
                    interview.StatusChangeOriginatorName,
                    this.GetUserRole(interview.StatusChangeOriginatorRole),
                    this.GetResponsibleName(interview.Status, interview.InterviewerName, interview.SupervisorName, interview.StatusChangeOriginatorName),
                    this.GetResponsibleRole(interview.Status, interview.StatusChangeOriginatorRole, interview.InterviewerName),
                    interview.Timestamp.ToString(ExportFormatSettings.ExportDateFormat, CultureInfo.InvariantCulture),
                    interview.Timestamp.ToString("T", CultureInfo.InvariantCulture)
                };
                result.Add(resultRow.ToArray());
            }
            return result;
        }

        private string GetResponsibleName(InterviewExportedAction status, string interviewerName, string supervisorName, string statusChangeOriginatorName)
        {
            switch (status)
            {
                case InterviewExportedAction.Created:
                    return statusChangeOriginatorName;
                case InterviewExportedAction.SupervisorAssigned:
                case InterviewExportedAction.Completed:
                case InterviewExportedAction.RejectedByHeadquarter:
                case InterviewExportedAction.UnapprovedByHeadquarter:
                    return supervisorName;
                case InterviewExportedAction.ApprovedBySupervisor:
                    return "any headquarters";
                case InterviewExportedAction.ApprovedByHeadquarter:
                    return String.Empty;
            }

            return interviewerName;
        }

        private string GetResponsibleRole(InterviewExportedAction status, UserRoles statusChangeOriginatorRole, string interviewerName)
        {
            switch (status)
            {
                case InterviewExportedAction.Created:
                    return GetUserRole(statusChangeOriginatorRole);
                case InterviewExportedAction.SupervisorAssigned:
                case InterviewExportedAction.Completed:
                case InterviewExportedAction.RejectedByHeadquarter:
                case InterviewExportedAction.UnapprovedByHeadquarter:
                case InterviewExportedAction.OpenedBySupervisor:
                case InterviewExportedAction.ClosedBySupervisor:
                    return FileBasedDataExportRepositoryWriterMessages.Supervisor;
                case InterviewExportedAction.ApprovedBySupervisor:
                    return FileBasedDataExportRepositoryWriterMessages.Headquarter;
                case InterviewExportedAction.ApprovedByHeadquarter:
                    return String.Empty;
                case InterviewExportedAction.InterviewerAssigned:
                    if (string.IsNullOrWhiteSpace(interviewerName))
                        return string.Empty;
                    break;
            }

            return FileBasedDataExportRepositoryWriterMessages.Interviewer;
        }

        private string GetUserRole(UserRoles userRole)
        {
            switch (userRole)
            {
                case UserRoles.Interviewer:
                    return FileBasedDataExportRepositoryWriterMessages.Interviewer;
                case UserRoles.Supervisor:
                    return FileBasedDataExportRepositoryWriterMessages.Supervisor;
                case UserRoles.Headquarter:
                    return FileBasedDataExportRepositoryWriterMessages.Headquarter;
                case UserRoles.Administrator:
                    return FileBasedDataExportRepositoryWriterMessages.Administrator;
                case UserRoles.ApiUser:
                    return FileBasedDataExportRepositoryWriterMessages.ApiUser;

            }
            return FileBasedDataExportRepositoryWriterMessages.UnknownRole;
        }
    }
}