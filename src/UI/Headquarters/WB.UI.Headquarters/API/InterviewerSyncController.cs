﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Hosting;
using System.Web.Http;
using Flurl.Http.Content;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernel.Structures.TabletInformation;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Web.Code;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.User;
using WB.UI.Headquarters.Resources;

namespace WB.UI.Headquarters.API
{
    public class InterviewerSyncController : BaseApiController
    { 
        private readonly ISyncProtocolVersionProvider syncVersionProvider;
        private readonly IIdentityManager identityManager;
        private readonly IPlainInterviewFileStorage plainFileRepository;
        private readonly IFileSystemAccessor fileSystemAccessor; 
        private readonly ITabletInformationService tabletInformationService;
        private readonly IInterviewPackagesService incomingSyncPackagesQueue;
        
        private readonly IUserViewFactory userViewFactory;
        private readonly IAndroidPackageReader androidPackageReader;

        private string ResponseInterviewerFileName = "interviewer.apk";
        private string CapiFileName = "wbcapi.apk";
        private string pathToSearchVersions = ("~/Client/");


        public InterviewerSyncController(ICommandService commandService,
            IIdentityManager identityManager,
            ILogger logger,
            IPlainInterviewFileStorage plainFileRepository,
            IFileSystemAccessor fileSystemAccessor,
            ISyncProtocolVersionProvider syncVersionProvider,
            ITabletInformationService tabletInformationService,
            IInterviewPackagesService incomingSyncPackagesQueue, 
            IUserViewFactory userViewFactory,
            IAndroidPackageReader androidPackageReader)
            : base(commandService, logger)
        {
            this.identityManager = identityManager;
            this.plainFileRepository = plainFileRepository;
            this.fileSystemAccessor = fileSystemAccessor;
            this.tabletInformationService = tabletInformationService;
            this.incomingSyncPackagesQueue = incomingSyncPackagesQueue;
            this.syncVersionProvider = syncVersionProvider;
            this.userViewFactory = userViewFactory;
            this.androidPackageReader = androidPackageReader;
        }

        [HttpGet]
        [ApiBasicAuth(new[] {UserRoles.Interviewer})]
        [Obsolete]
        public HttpResponseMessage GetHandshakePackage(string clientId, string androidId, Guid? clientRegistrationId, int version = 0)
        {
            int supervisorRevisionNumber = this.syncVersionProvider.GetProtocolVersion();

            this.Logger.Info(string.Format("Old version client. Client has protocol version {0} but current app protocol is {1} ", version, supervisorRevisionNumber));

            return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, TabletSyncMessages.InterviewerIsNotCompatibleWithThisVersion);
        }

        [HttpGet]
        [ApiBasicAuth(new[] {UserRoles.Interviewer})]
        public bool CheckExpectedDevice(string deviceId) =>
            string.IsNullOrEmpty(this.identityManager.CurrentUserDeviceId) || this.identityManager.CurrentUserDeviceId == deviceId;

        [HttpPost]
        [ApiBasicAuth(new[] { UserRoles.Interviewer })]
        public HttpResponseMessage GetHandshakePackage(HandshakePackageRequest request)
        {
            throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.NotAcceptable)
            {
                ReasonPhrase = TabletSyncMessages.InterviewerIsNotCompatibleWithThisVersion
            });
        }

        [HttpPost]
        [ApiBasicAuth(new[] { UserRoles.Interviewer })]
        public HttpResponseMessage PostFile(PostFileRequest request)
        {
            this.plainFileRepository.StoreInterviewBinaryData(request.InterviewId, request.FileName, Convert.FromBase64String(request.Data));
            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        [ApiBasicAuth(new[] { UserRoles.Interviewer })]
        public HttpResponseMessage PostPackage(PostPackageRequest request)
        {
            this.incomingSyncPackagesQueue.StoreOrProcessPackage(item: request.SynchronizationPackage);

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetLatestVersion()
        {
            string pathToFile = this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(this.pathToSearchVersions), this.CapiFileName);

            if (this.fileSystemAccessor.IsFileExists(pathToFile))
            {
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StreamContent(this.fileSystemAccessor.ReadFile(pathToFile))
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.android.package-archive");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = this.ResponseInterviewerFileName
                };

                return response;
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, TabletSyncMessages.FileWasNotFound);
        }

        [HttpGet]
        [AllowAnonymous]
        public bool CheckNewVersion(int versionCode)
        {
            string pathToInterviewerApp =
                this.fileSystemAccessor.CombinePath(HostingEnvironment.MapPath(this.pathToSearchVersions), this.CapiFileName);

            int? interviewerApkVersion = !this.fileSystemAccessor.IsFileExists(pathToInterviewerApp)
                ? null
                : this.androidPackageReader.Read(pathToInterviewerApp).Version;
            
            return interviewerApkVersion.HasValue && (interviewerApkVersion.Value > versionCode);
        }

        [HttpPost]
        [AllowAnonymous]
        public void PostInfoPackage(TabletInformationPackage tabletInformationPackage)
        {
            var user = this.userViewFactory.Load(new UserViewInputModel(tabletInformationPackage.AndroidId));

            this.tabletInformationService.SaveTabletInformation(
                content: Convert.FromBase64String(tabletInformationPackage.Content),
                androidId: tabletInformationPackage.AndroidId,
                user: user);

            //log record
        }
    }
}