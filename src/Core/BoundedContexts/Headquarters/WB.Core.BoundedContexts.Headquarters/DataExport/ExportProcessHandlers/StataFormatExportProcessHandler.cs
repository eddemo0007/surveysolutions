﻿using Microsoft;
using WB.Core.BoundedContexts.Headquarters.DataExport.DataExportProcess;
using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.BoundedContexts.Headquarters.DataExport.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveyManagement.Services.Export;
using WB.Core.SharedKernels.SurveyManagement.Views.InterviewHistory;
using IFilebasedExportedDataAccessor = WB.Core.BoundedContexts.Headquarters.DataExport.Accessors.IFilebasedExportedDataAccessor;

namespace WB.Core.BoundedContexts.Headquarters.DataExport.ExportProcessHandlers
{
    internal class StataFormatExportProcessHandler : IExportProcessHandler<AllDataExportProcess>, IExportProcessHandler<ApprovedDataExportProcess>
    {
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly ITabularFormatExportService tabularFormatExportService;
        private readonly IArchiveUtils archiveUtils;
        private const string allDataFolder = "AllData";
        private const string approvedDataFolder = "ApprovedData";
        private const string temporaryTabularDataForStataExportFolder = "TemporaryTabularDataForStataExport";
        private readonly string pathToExportedData;
        private readonly IFilebasedExportedDataAccessor filebasedExportedDataAccessor;
        private readonly ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService;
        private readonly IDataExportProcessesService dataExportProcessesService;

        public StataFormatExportProcessHandler(
            IFileSystemAccessor fileSystemAccessor, 
            ITabularFormatExportService tabularFormatExportService, 
            IArchiveUtils archiveUtils, 
            IFilebasedExportedDataAccessor filebasedExportedDataAccessor,
            ITabularDataToExternalStatPackageExportService tabularDataToExternalStatPackageExportService,
            InterviewDataExportSettings interviewDataExportSettings, IDataExportProcessesService dataExportProcessesService)
        {
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabularFormatExportService = tabularFormatExportService;
            this.archiveUtils = archiveUtils;
            this.filebasedExportedDataAccessor = filebasedExportedDataAccessor;
            this.tabularDataToExternalStatPackageExportService = tabularDataToExternalStatPackageExportService;
            this.dataExportProcessesService = dataExportProcessesService;

            this.pathToExportedData = fileSystemAccessor.CombinePath(interviewDataExportSettings.DirectoryPath, temporaryTabularDataForStataExportFolder);

            if (!fileSystemAccessor.IsDirectoryExists(this.pathToExportedData))
                fileSystemAccessor.CreateDirectory(this.pathToExportedData);
        }

        public void ExportData(AllDataExportProcess process)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity), allDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => this.dataExportProcessesService.UpdateDataExportProgress(process.DataExportProcessId, donePercent);

            this.tabularFormatExportService
                .ExportInterviewsInTabularFormatAsync(process.QuestionnaireIdentity, folderForDataExport, exportProggress);

            exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => this.dataExportProcessesService.UpdateDataExportProgress(process.DataExportProcessId, 50 + (donePercent/2));

            var statsFiles = tabularDataToExternalStatPackageExportService.CreateAndGetStataDataFilesForQuestionnaire(process.QuestionnaireIdentity.QuestionnaireId,
                process.QuestionnaireIdentity.Version, fileSystemAccessor.GetFilesInDirectory(folderForDataExport), exportProggress);

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedData(
                process.QuestionnaireIdentity,
                DataExportFormat.STATA);

            RecreateExportArchive(statsFiles, archiveFilePath);
        }

        public void ExportData(ApprovedDataExportProcess process)
        {
            string folderForDataExport =
              this.fileSystemAccessor.CombinePath(GetFolderPathOfDataByQuestionnaire(process.QuestionnaireIdentity), approvedDataFolder);

            this.ClearFolder(folderForDataExport);

            var exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => this.dataExportProcessesService.UpdateDataExportProgress(process.DataExportProcessId, donePercent/2);

            this.tabularFormatExportService
                .ExportApprovedInterviewsInTabularFormatAsync(process.QuestionnaireIdentity, folderForDataExport, exportProggress);

            exportProggress = new Progress<int>();
            exportProggress.ProgressChanged += (sender, donePercent) => this.dataExportProcessesService.UpdateDataExportProgress(process.DataExportProcessId, 50 + +(donePercent / 2));

            var statsFiles = tabularDataToExternalStatPackageExportService.CreateAndGetStataDataFilesForQuestionnaire(process.QuestionnaireIdentity.QuestionnaireId,
                process.QuestionnaireIdentity.Version, fileSystemAccessor.GetFilesInDirectory(folderForDataExport), exportProggress);

            var archiveFilePath = this.filebasedExportedDataAccessor.GetArchiveFilePathForExportedApprovedData(
                process.QuestionnaireIdentity,
                DataExportFormat.STATA);

            RecreateExportArchive(statsFiles, archiveFilePath);
        }

        private string GetFolderPathOfDataByQuestionnaire(QuestionnaireIdentity questionnaireIdentity)
        {
            return this.fileSystemAccessor.CombinePath(this.pathToExportedData,
                $"{questionnaireIdentity.QuestionnaireId}_{questionnaireIdentity.Version}");
        }

        private void RecreateExportArchive(string[] stataFiles, string archiveFilePath)
        {
            if (this.fileSystemAccessor.IsFileExists(archiveFilePath))
            {
                this.fileSystemAccessor.DeleteFile(archiveFilePath);
            }
            this.archiveUtils.ZipFiles(stataFiles, archiveFilePath);
        }


        private void ClearFolder(string folderName)
        {
            if (this.fileSystemAccessor.IsDirectoryExists(folderName))
                this.fileSystemAccessor.DeleteDirectory(folderName);

            this.fileSystemAccessor.CreateDirectory(folderName);
        }
    }
}