using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Platform;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Structures.Synchronization.SurveyManagement;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views
{
    public class SynchronizationViewModel : MvxNotifyPropertyChanged
    {
        private readonly ISynchronizationService synchronizationService;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IQuestionnaireModelBuilder questionnaireModelBuilder;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly ICommandService commandService;
        private readonly ICapiDataSynchronizationService capiDataSynchronizationService;
        private readonly ISyncPackageIdsStorage syncPackageIdsStorage;
        private readonly ICapiCleanUpService capiCleanUpService;
        private readonly IPrincipal principal;
        private readonly IJsonUtils jsonUtils;
        private readonly IAsyncPlainStorage<QuestionnireInfo> plainStorageQuestionnireInfo;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private CancellationTokenSource synchronizationCancellationTokenSource;

        public SynchronizationViewModel(
            ISynchronizationService synchronizationService,
            IViewModelNavigationService viewModelNavigationService,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IQuestionnaireModelBuilder questionnaireModelBuilder,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository,
            IPlainQuestionnaireRepository questionnaireRepository,
            ICommandService commandService,
            ICapiDataSynchronizationService capiDataSynchronizationService,
            ISyncPackageIdsStorage syncPackageIdsStorage,
            ICapiCleanUpService capiCleanUpService,
            IPrincipal principal,
            IJsonUtils jsonUtils,
            IAsyncPlainStorage<QuestionnireInfo> plainStorageQuestionnireInfo,
            IPlainInterviewFileStorage plainInterviewFileStorage)
        {
            this.synchronizationService = synchronizationService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.questionnaireModelBuilder = questionnaireModelBuilder;
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.commandService = commandService;
            this.capiDataSynchronizationService = capiDataSynchronizationService;
            this.syncPackageIdsStorage = syncPackageIdsStorage;
            this.capiCleanUpService = capiCleanUpService;
            this.principal = principal;
            this.jsonUtils = jsonUtils;
            this.plainStorageQuestionnireInfo = plainStorageQuestionnireInfo;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
        }

        private CancellationToken Token { get { return this.synchronizationCancellationTokenSource.Token; } }

        private SychronizationStatistics statistics;
        public SychronizationStatistics Statistics { get { return statistics; } set { statistics = value; this.RaisePropertyChanged(); } }

        private SynchronizationStatus status;
        public SynchronizationStatus Status
        {
            get { return this.status; }
            set { this.status = value; this.RaisePropertyChanged(); }
        }

        private bool isSynchronizationInfoShowed;
        public bool IsSynchronizationInfoShowed
        {
            get { return this.isSynchronizationInfoShowed; }
            set { this.isSynchronizationInfoShowed = value; this.RaisePropertyChanged(); }
        }

        private bool isSynchronizationInProgress;
        public bool IsSynchronizationInProgress
        {
            get { return this.isSynchronizationInProgress; }
            set { this.isSynchronizationInProgress = value; this.RaisePropertyChanged(); }
        }

        private string processOperation;
        public string ProcessOperation
        {
            get { return this.processOperation; }
            set
            {
                if (this.processOperation == value) return;

                this.processOperation = value;
                this.RaisePropertyChanged();
            }
        }

        public string processOperationDescription;
        public string ProcessOperationDescription
        {
            get { return this.processOperationDescription; }
            set { this.processOperationDescription = value; this.RaisePropertyChanged(); }
        }

        public IMvxCommand CancelSynchronizationCommand
        {
            get { return new MvxCommand(this.CancelSynchronizaion); }
        }

        public async Task SynchronizeAsync()
        {
            this.statistics = new SychronizationStatistics();
            this.synchronizationCancellationTokenSource = new CancellationTokenSource();

            this.Status = SynchronizationStatus.Download;
            this.IsSynchronizationInfoShowed = true;
            this.IsSynchronizationInProgress = true;
            try
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_UserAuthentication_Title,
                    InterviewerUIResources.Synchronization_UserAuthentication_Description);

                await this.synchronizationService.CheckInterviewerCompatibilityWithServerAsync(this.Token);

                if (!await this.synchronizationService.IsDeviceLinkedToCurrentInterviewerAsync(this.Token))
                {
                    this.viewModelNavigationService.NavigateTo<RelinkDeviceViewModel>();
                    return;
                }

                var packagesByInterviews = await this.synchronizationService.GetInterviewPackagesAsync(this.syncPackageIdsStorage.GetLastStoredPackageId(), this.Token);
                var completedInterviews = this.capiDataSynchronizationService.GetItemsToPush();

                this.statistics.TotalNewInterviewsCount = packagesByInterviews.Interviews.Count(interview => !interview.IsRejected);
                this.statistics.TotalRejectedInterviewsCount = packagesByInterviews.Interviews.Count(interview => interview.IsRejected);
                this.statistics.TotalCompletedInterviewsCount = completedInterviews.Count;

                this.Status = SynchronizationStatus.Upload;
                await this.UploadCompletedInterviewsAsync(completedInterviews);

                this.Status = SynchronizationStatus.Download;
                await this.DownloadCensusAsync();
                await this.DownloadInterviewPackagesAsync(packagesByInterviews);

                this.Status = SynchronizationStatus.Success;
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Success_Title,
                    InterviewerUIResources.Synchronization_Success_Description);
            }
            catch (RestException ex)
            {
                switch (ex.Type)
                {
                        case RestExceptionType.RequestCancelled:
                        this.IsSynchronizationInfoShowed = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                Mvx.Trace(MvxTraceLevel.Error, ex.Message);
                this.Status = SynchronizationStatus.Fail;
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Fail_Title,
                    InterviewerUIResources.Synchronization_Fail_Description);
            }
            finally
            {
                this.Statistics = statistics;
                this.IsSynchronizationInProgress = false;
            }
        }

        private void SetProgressOperation(string title, string description)
        {
            this.ProcessOperation = title;
            this.ProcessOperationDescription = description;
        }

        private async Task DownloadCensusAsync()
        {
            var censusQuestionnaires = await this.synchronizationService.GetCensusQuestionnairesAsync(this.Token);

            var processedQuestionnaires = 0;
            foreach (var censusQuestionnaire in censusQuestionnaires)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                            InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                                processedQuestionnaires, censusQuestionnaires.Count,
                                InterviewerUIResources.Synchronization_Questionnaires));

                await this.DownloadQuestionnaireAsync(censusQuestionnaire);

                processedQuestionnaires++;
            }
        }

        private async Task DownloadQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity)
        {
            if (this.plainStorageQuestionnireInfo.GetById(questionnaireIdentity.ToString()) == null)
            {
                var questionnaireApiView = await this.synchronizationService.GetQuestionnaireAsync(
                   questionnaire: questionnaireIdentity,
                   onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                   token: this.Token);

                await this.SaveQuestionnaireAsync(questionnaireIdentity, questionnaireApiView);   
            }

            if (!this.questionnaireAssemblyFileAccessor.IsQuestionnaireAssemblyExists(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version))
            {
                var questionnaireAssembly = await this.synchronizationService.GetQuestionnaireAssemblyAsync(
                    questionnaire: questionnaireIdentity,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId,
                    questionnaireIdentity.Version, questionnaireAssembly);
            }
        }

        private async Task DownloadInterviewPackagesAsync(InterviewPackagesApiView interviewPackages)
        {
            var lastKnownPackageId = this.syncPackageIdsStorage.GetLastStoredPackageId();
            
            var listOfProcessedInterviews = new List<Guid>();
            foreach (var interviewPackage in interviewPackages.Packages)
            {
                this.SetProgressOperation(InterviewerUIResources.Synchronization_Download_Title,
                    InterviewerUIResources.Synchronization_Download_Description_Format.FormatString(
                        listOfProcessedInterviews.Count, interviewPackages.Interviews.Count,
                        InterviewerUIResources.Synchronization_Interviews));

                var package = await this.synchronizationService.GetInterviewPackageAsync(
                    packageId: interviewPackage.Id,
                    previousSuccessfullyHandledPackageId: lastKnownPackageId,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                lastKnownPackageId = interviewPackage.Id;

                await this.SaveInterviewAsync(package, interviewPackage);
                await this.synchronizationService.LogPackageAsSuccessfullyHandledAsync(lastKnownPackageId, this.Token);

                if (interviewPackage.ItemType == SyncItemType.Interview)
                {
                    if (!listOfProcessedInterviews.Contains(interviewPackage.InterviewId)) listOfProcessedInterviews.Add(interviewPackage.InterviewId);

                    var interviewInfo = interviewPackages.Interviews.Find(interview => interview.Id == interviewPackage.InterviewId);
                    await this.DownloadQuestionnaireAsync(interviewInfo.QuestionnaireIdentity);

                    if (interviewInfo.IsRejected)
                        this.Statistics.RejectedInterviewsCount++;
                    else
                        this.Statistics.NewInterviewsCount++;  
                };
            }
        }

        private async Task UploadCompletedInterviewsAsync(IList<ChangeLogRecordWithContent> dataByChuncks)
        {
            foreach (var chunckDescription in dataByChuncks)
            {
                this.SetProgressOperation(
                    InterviewerUIResources.Synchronization_Upload_Title_Format.FormatString(InterviewerUIResources.Synchronization_Upload_CompletedAssignments_Text),
                    InterviewerUIResources.Synchronization_Upload_Description_Format.FormatString(
                        this.Statistics.CompletedInterviewsCount, this.Statistics.TotalCompletedInterviewsCount,
                        InterviewerUIResources.Synchronization_Upload_Interviews_Text));

                await this.UploadImagesByCompletedInterview(chunckDescription.EventSourceId);

                await this.synchronizationService.UploadInterviewAsync(
                    interviewId: chunckDescription.EventSourceId,
                    content: chunckDescription.Content,
                    onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                    token: this.Token);

                await this.RemoveInterviewAsync(chunckDescription);

                this.Statistics.CompletedInterviewsCount++;
            }
        }

        private async Task UploadImagesByCompletedInterview(Guid interviewId)
        {
            await Task.Run(async () =>
            {
                var interviewImages =  this.plainInterviewFileStorage.GetBinaryFilesForInterview(interviewId);
                foreach (var image in interviewImages)
                {
                    await this.synchronizationService.UploadInterviewImageAsync(
                        interviewId: image.InterviewId,
                        fileName: image.FileName,
                        fileData: image.GetData(),
                        onDownloadProgressChanged: (progressPercentage, bytesReceived, totalBytesToReceive) => { },
                        token: this.Token);
                    this.plainInterviewFileStorage.RemoveInterviewBinaryData(image.InterviewId, image.FileName);
                }
            });
        }

        private async Task SaveInterviewAsync(InterviewSyncPackageDto package, SynchronizationChunkMeta synchronizationChunkMeta)
        {
            await Task.Run(() =>
            {
                this.capiDataSynchronizationService.ProcessDownloadedInterviewPackages(package, synchronizationChunkMeta.ItemType);
                this.syncPackageIdsStorage.Append(package.PackageId, synchronizationChunkMeta.SortIndex);
            });
        }

        private async Task RemoveInterviewAsync(ChangeLogRecordWithContent chunckDescription)
        {
            await Task.Run(() =>
            {
                this.capiCleanUpService.DeleteInterview(chunckDescription.EventSourceId);
            });
        }

        private async Task SaveQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, QuestionnaireApiView questionnaireApiView)
        {
            await Task.Run(() =>
            {
                var questionnaireDocument = this.jsonUtils.Deserialize<QuestionnaireDocument>(questionnaireApiView.QuestionnaireDocument);
                var questionnaireModel = this.questionnaireModelBuilder.BuildQuestionnaireModel(questionnaireDocument);
                this.questionnaireModelRepository.Store(questionnaireModel, questionnaireIdentity.ToString());
                this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
                this.commandService.Execute(new RegisterPlainQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireApiView.AllowCensus, string.Empty));
            });

            await this.plainStorageQuestionnireInfo.StoreAsync(new QuestionnireInfo()
            {
                Id = questionnaireIdentity.ToString(),
                AllowCensus = questionnaireApiView.AllowCensus
            });
        }

        public void CancelSynchronizaion()
        {
            this.synchronizationCancellationTokenSource.Cancel();
        }    
    }
}