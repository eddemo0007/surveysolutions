﻿using WB.Core.BoundedContexts.Headquarters.DataExport.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IExportFileNameService
    {
        string GetFileNameForBatchUploadByQuestionnaire(QuestionnaireIdentity identity);
        string GetFolderNameForParaDataByQuestionnaire(QuestionnaireIdentity identity, string pathToHistoryFiles);
        string GetFileNameForDdiByQuestionnaire(QuestionnaireIdentity identity, string pathToDdiMetadata);
        string GetFileNameForTabByQuestionnaire(QuestionnaireIdentity identity, string pathToExportedData, DataExportFormat format, string statusSuffix);
    }
}