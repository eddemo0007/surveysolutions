﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Messages;
using WB.Core.SharedKernels.Enumerator.OfflineSync.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public partial class OfflineSynchronizationService
    {
        private readonly IOfflineSyncClient syncClient;
        private readonly IInterviewerPrincipal principal;
        private readonly IPlainStorage<InterviewView> interviews;

        public OfflineSynchronizationService(
            IOfflineSyncClient syncClient,
            IInterviewerPrincipal principal,
            IPlainStorage<InterviewView> interviews)
        {
            this.syncClient = syncClient;
            this.principal = principal;
            this.interviews = interviews;
        }

        public Task UploadInterviewAsync(Guid interviewId, InterviewPackageApiView completedInterview,
            Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            var interviewKey = this.interviews.GetById(interviewId.FormatGuid())?.InterviewKey;
            return this.syncClient.SendAsync(new UploadInterviewRequest
            {
                Interview = completedInterview,
                InterviewKey = interviewKey
            }, token);
        }

        public Task UploadInterviewImageAsync(Guid interviewId, string fileName, byte[] fileData,
            Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            return Task.CompletedTask; //TODO: Implement
        }

        public Task UploadInterviewAudioAsync(Guid interviewId, string fileName, string contentType, byte[] fileData,
            Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            return Task.CompletedTask;//TODO: Implement
        }

        public async Task<List<string>> GetAttachmentContentsAsync(QuestionnaireIdentity questionnaire,
            Action<decimal, long, long> onDownloadProgressChanged,
            CancellationToken token)
        {
            var response = await syncClient.SendAsync<GetAttachmentContentsRequest, GetAttachmentContentsResponse>(
                new GetAttachmentContentsRequest {QuestionnaireIdentity = questionnaire}, token);

            return response.AttachmentContents;
        }

        public async Task<AttachmentContent> GetAttachmentContentAsync(string contentId,
            Action<decimal, long, long> onDownloadProgressChanged, CancellationToken token)
        {
            var response = await syncClient.SendAsync<GetAttachmentContentRequest, GetAttachmentContentResponse>(
                new GetAttachmentContentRequest { ContentId = contentId }, token);

            return response.Content;
        }

        public async Task<List<QuestionnaireIdentity>> GetServerQuestionnairesAsync(CancellationToken cancellationToken)
        {
            var response = await this.syncClient.SendAsync<GetQuestionnaireList.Request, GetQuestionnaireList.Response>(
                   new GetQuestionnaireList.Request(), cancellationToken);

            return response.Questionnaires;
        }

        public async Task<List<TranslationDto>> GetQuestionnaireTranslationAsync(QuestionnaireIdentity questionnaireIdentity,
            CancellationToken cancellationToken)
        {
            var response = await this.syncClient
                .SendAsync<GetQuestionnaireTranslationRequest, GetQuestionnaireTranslationResponse>(
                    new GetQuestionnaireTranslationRequest
                    {
                        QuestionnaireIdentity = questionnaireIdentity
                    }, cancellationToken);

            return response.Translations;
        }

        public async Task<CompanyLogoInfo> GetCompanyLogo(string storedClientEtag, CancellationToken cancellationToken)
        {
            var response = await syncClient.SendAsync<GetCompanyLogoRequest, GetCompanyLogoResponse>(
                new GetCompanyLogoRequest
                {
                    Etag = storedClientEtag
                }, cancellationToken);
            return response.LogoInfo;
        }

        public Task SendSyncStatisticsAsync(SyncStatisticsApiView statistics,
            CancellationToken token,
            RestCredentials credentials)
        {
            //TODO: Implement
            return Task.CompletedTask;
        }

        public Task SendUnexpectedExceptionAsync(UnexpectedExceptionApiView exception, CancellationToken token)
        {
            //TODO: Implement
            return Task.CompletedTask;
        }

        public Task<List<MapView>> GetMapList(CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<MapView>());
        }

        public Task<RestStreamResult> GetMapContentStream(string mapName, CancellationToken cancellationToken)
        {
            return Task.FromResult<RestStreamResult>(null);
        }

        public Task<Guid> GetCurrentSupervisor(CancellationToken token, RestCredentials credentials)
        {
            return Task.FromResult(this.principal.CurrentUserIdentity.SupervisorId);
        }

        public Task<bool> IsAutoUpdateEnabledAsync(CancellationToken token)
        {
            return Task.FromResult(false);
        }

        public Task UploadAuditLogEntityAsync(AuditLogEntitiesApiView auditLogEntity, CancellationToken cancellationToken)
        {
            return this.syncClient.SendAsync(new UploadAuditLogEntityRequest
            {
                AuditLogEntity = auditLogEntity
            }, cancellationToken);
        }

        public Task<List<Guid>> CheckObsoleteInterviewsAsync(List<ObsoletePackageCheck> checks,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<Guid>());
        }

        public async Task<AssignmentApiDocument> GetAssignmentAsync(int id, CancellationToken cancellationToken)
        {
            var response = await
                syncClient.SendAsync<GetAssignmentRequest, GetAssignmentResponse>(new GetAssignmentRequest { Id = id },
                    cancellationToken);

            return response.Assignment;
        }

        public Task LogAssignmentAsHandledAsync(int id, CancellationToken cancellationToken)
        {
            return syncClient.SendAsync(new LogAssignmentAsHandledRequest { Id = id }, cancellationToken);
        }

        public async Task<List<AssignmentApiView>> GetAssignmentsAsync(CancellationToken cancellationToken)
        {
            var response = await
                syncClient.SendAsync<GetAssignmentsRequest, GetAssignmentsResponse>(new GetAssignmentsRequest { UserId = principal.CurrentUserIdentity.UserId },
                    cancellationToken);

            return response.Assignments;
        }
    }
}
