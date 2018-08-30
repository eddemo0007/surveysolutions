﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Main.Core.Entities.SubEntities;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.SynchronizationLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.Synchronization.MetaInfo;
using WB.UI.Headquarters.API.DataCollection.Interviewer;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.DataCollection.Supervisor.v1
{
    [ApiBasicAuth(new[] { UserRoles.Supervisor })]
    public class InterviewsApiV1Controller : SupervisorInterviewsControllerBase
    {
        private readonly IImageFileStorage imageFileStorage;
        private readonly IAudioFileStorage audioFileStorage;

        public InterviewsApiV1Controller(IImageFileStorage imageFileStorage, IAudioFileStorage audioFileStorage, IAuthorizedUser authorizedUser, IInterviewInformationFactory interviewsFactory, IInterviewPackagesService packagesService, ICommandService commandService, IMetaInfoBuilder metaBuilder, IJsonAllTypesSerializer synchronizationSerializer, IHeadquartersEventStore eventStore) : base(imageFileStorage, audioFileStorage, authorizedUser, interviewsFactory, packagesService, commandService, metaBuilder, synchronizationSerializer, eventStore)
        {
            this.imageFileStorage = imageFileStorage;
            this.audioFileStorage = audioFileStorage;
        }

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviews)]
        public override HttpResponseMessage Get() => base.Get();

        [HttpGet]
        [WriteToSyncLog(SynchronizationLogType.GetInterviewV3)]
        public JsonResult<List<CommittedEvent>> Details(Guid id) => base.DetailsV3(id);

        [WriteToSyncLog(SynchronizationLogType.PostInterviewV3)]
        [HttpPost]
        public HttpResponseMessage Post(InterviewPackageApiView package) => base.PostV3(package);

        [WriteToSyncLog(SynchronizationLogType.InterviewProcessed)]
        [HttpPost]
        public override void LogInterviewAsSuccessfullyHandled(Guid id) => base.LogInterviewAsSuccessfullyHandled(id);

        [HttpPost]
        public override void PostImage(PostFileRequest request) => base.PostImage(request);

        [HttpPost]
        public override void PostAudio(PostFileRequest request) => base.PostAudio(request);

        [WriteToSyncLog(SynchronizationLogType.CheckIsPackageDuplicated)]
        [HttpPost]
        public InterviewUploadState GetInterviewUploadState(Guid id, [FromBody] EventStreamSignatureTag eventStreamSignatureTag)
        {
            var doesEventsExists = this.packagesService.IsPackageDuplicated(eventStreamSignatureTag);

            var binaries = this.imageFileStorage.GetBinaryFilesForInterview(id)
                .Union(this.audioFileStorage.GetBinaryFilesForInterview(id))
                .Select(bf => bf.FileName);

            return new InterviewUploadState
            {
                IsEventsUploaded = doesEventsExists,
                BinaryFilesNames = binaries.ToHashSet()
            };
        }
    }
}
