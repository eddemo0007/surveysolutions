using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.Code;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer
{
    public class InterviewsControllerBase : ApiController
    {
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly IIdentityManager identityManager;
        protected readonly IInterviewPackagesService interviewPackagesService;
        protected readonly ICommandService commandService;
        protected readonly IMetaInfoBuilder metaBuilder;
        protected readonly IJsonAllTypesSerializer synchronizationSerializer;
        protected readonly IInterviewInformationFactory interviewsFactory;

        public InterviewsControllerBase(
            IPlainInterviewFileStorage plainInterviewFileStorage,
            IIdentityManager identityManager,
            IInterviewInformationFactory interviewsFactory,
            IInterviewPackagesService interviewPackagesService,
            ICommandService commandService,
            IMetaInfoBuilder metaBuilder,
            IJsonAllTypesSerializer synchronizationSerializer)
        {
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.identityManager = identityManager;
            this.interviewsFactory = interviewsFactory;
            this.interviewPackagesService = interviewPackagesService;
            this.commandService = commandService;
            this.metaBuilder = metaBuilder;
            this.synchronizationSerializer = synchronizationSerializer;
        }

        [WriteToSyncLog(SynchronizationLogType.GetInterviews)]
        public virtual HttpResponseMessage Get()
        {
            var resultValue = this.interviewsFactory.GetInProgressInterviews(this.identityManager.CurrentUserId)
                .Select(interview => new InterviewApiView()
                {
                    Id = interview.Id,
                    QuestionnaireIdentity = interview.QuestionnaireIdentity,
                    IsRejected = interview.IsRejected
                }).ToList();

            var response = this.Request.CreateResponse(resultValue);
            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                Public = false,
                NoCache = true
            };

            return response;
        }
        
        [WriteToSyncLog(SynchronizationLogType.InterviewProcessed)]
        public virtual void LogInterviewAsSuccessfullyHandled(Guid id)
        {
            this.commandService.Execute(new MarkInterviewAsReceivedByInterviewer(id, this.identityManager.CurrentUserId));
        }
        
        public virtual void PostImage(PostFileRequest request)
        {
            this.plainInterviewFileStorage.StoreInterviewBinaryData(request.InterviewId, request.FileName,
                Convert.FromBase64String(request.Data));
        }
    }
}