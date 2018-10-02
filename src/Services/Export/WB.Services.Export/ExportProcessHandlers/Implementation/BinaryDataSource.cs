﻿using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;
using WB.Services.Export.Infrastructure;
using WB.Services.Export.Interview;
using WB.Services.Export.Questionnaire.Services;
using WB.Services.Export.Services;
using WB.Services.Export.Utils;


namespace WB.Services.Export.ExportProcessHandlers.Implementation
{
    internal class BinaryDataSource : IBinaryDataSource
    {
        private readonly IInterviewFactory interviewFactory;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ITenantApi<IHeadquartersApi> tenantApi;
        private readonly ILogger logger;
        private readonly IOptions<InterviewDataExportSettings> interviewDataExportSettings;

        public BinaryDataSource(
            IOptions<InterviewDataExportSettings> interviewDataExportSettings,
            IInterviewFactory interviewFactory,
            IQuestionnaireStorage questionnaireStorage,
            ITenantApi<IHeadquartersApi> tenantApi,
            ILogger<BinaryDataSource> logger)
        {
            this.interviewFactory = interviewFactory;
            this.questionnaireStorage = questionnaireStorage;
            this.tenantApi = tenantApi;
            this.logger = logger;
            this.interviewDataExportSettings = interviewDataExportSettings;
        }

        public async Task ForEachMultimediaAnswerAsync(ExportSettings settings, Func<BinaryData, Task> action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var api = this.tenantApi.For(settings.Tenant);

            var interviewsToExport = await api.GetInterviewsToExportAsync(
                settings.QuestionnaireId,
                settings.InterviewStatus,
                settings.FromDate,
                settings.ToDate
            );

            var questionnaire = await this.questionnaireStorage
                .GetQuestionnaireAsync(settings.Tenant, settings.QuestionnaireId);

            var batchSize = interviewDataExportSettings.Value.MaxRecordsCountPerOneExportQuery;

            foreach (var interviewBatch in interviewsToExport.Batch(batchSize))
            {
                var interviewIds = interviewBatch.Select(i => i.Id).ToArray();

                var allMultimediaAnswers = this.interviewFactory.GetMultimediaAnswersByQuestionnaire(
                    settings.Tenant, questionnaire, interviewIds, cancellationToken).Result;

                cancellationToken.ThrowIfCancellationRequested();

                foreach (var answer in allMultimediaAnswers)
                {
                    try
                    {
                        byte[] content;

                        switch (answer.Type)
                        {
                            case MultimediaType.Image:
                                var imageContent = await api.GetInterviewImageAsync(answer.InterviewId, answer.Answer);
                                content = await imageContent.ReadAsByteArrayAsync();
                                break;
                            case MultimediaType.Audio:
                                var audioContent = await api.GetInterviewAudioAsync(answer.InterviewId, answer.Answer);
                                content = await audioContent.ReadAsByteArrayAsync();
                                break;
                            default:
                                continue;
                        }

                        await action(new BinaryData
                        {
                            InterviewId = answer.InterviewId,
                            Answer = answer.Answer,
                            Content = content
                        });
                    }
                    catch(ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
                    {
                        logger.LogWarning($"[{e.StatusCode}] Cannot download file for {answer.InterviewId} - {answer.Answer}");
                    }
                }
            }
        }
    }
}
