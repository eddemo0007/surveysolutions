﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Supervisor.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.WebApi;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Implementation.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.Services.Synchronization;
using WB.Core.SharedKernels.Enumerator.Views;

namespace WB.Core.BoundedContexts.Supervisor.Services.Implementation
{
    public class SupervisorSynchronizationProcess : SynchronizationProcessBase
    {
        private readonly ISupervisorSettings supervisorSettings;
        private readonly IPrincipal principal;
        private readonly IPlainStorage<SupervisorIdentity> supervisorsPlainStorage;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IPlainStorage<DeletedQuestionnaire> deletedQuestionnairesStorage;
        private readonly IPlainStorage<InterviewerDocument> interviewerViewRepository;
        private readonly ITechInfoSynchronizer techInfoSynchronizer;
        private readonly IPasswordHasher passwordHasher;

        public SupervisorSynchronizationProcess(ISupervisorSynchronizationService synchronizationService,
            IPlainStorage<SupervisorIdentity> supervisorsPlainStorage,
            IPlainStorage<InterviewView> interviewViewRepository,
            IPrincipal principal,
            ILogger logger,
            IUserInteractionService userInteractionService,
            IInterviewerQuestionnaireAccessor questionnairesAccessor,
            IInterviewerInterviewAccessor interviewFactory,
            IPlainStorage<InterviewMultimediaView> interviewMultimediaViewStorage,
            IPlainStorage<InterviewFileView> imagesStorage,
            CompanyLogoSynchronizer logoSynchronizer,
            AttachmentsCleanupService cleanupService,
            IPasswordHasher passwordHasher,
            IAssignmentsSynchronizer assignmentsSynchronizer,
            IQuestionnaireDownloader questionnaireDownloader,
            IHttpStatistician httpStatistician,
            IAssignmentDocumentsStorage assignmentsStorage,
            IAudioFileStorage audioFileStorage,
            ITabletDiagnosticService diagnosticService,
            ISupervisorSettings supervisorSettings,
            IAuditLogSynchronizer auditLogSynchronizer,
            IAuditLogService auditLogService,
            ILiteEventBus eventBus,
            IEnumeratorEventStorage eventStore,
            IPlainStorage<InterviewerDocument> interviewerViewRepository,
            ITechInfoSynchronizer techInfoSynchronizer,
            IPlainStorage<DeletedQuestionnaire> deletedQuestionnairesStorage,
            IPlainStorage<InterviewSequenceView, Guid> interviewSequenceViewRepository) : base(synchronizationService,
            interviewViewRepository, principal, logger,
            userInteractionService, questionnairesAccessor, interviewFactory, interviewMultimediaViewStorage,
            imagesStorage,
            logoSynchronizer, cleanupService, assignmentsSynchronizer, questionnaireDownloader,
            httpStatistician,
            assignmentsStorage, audioFileStorage, diagnosticService, auditLogSynchronizer, auditLogService,
            eventBus, eventStore, interviewSequenceViewRepository, supervisorSettings)
        {
            this.principal = principal;
            this.supervisorSettings = supervisorSettings;
            this.interviewerViewRepository = interviewerViewRepository;
            this.techInfoSynchronizer = techInfoSynchronizer;
            this.deletedQuestionnairesStorage = deletedQuestionnairesStorage;
            this.supervisorsPlainStorage = supervisorsPlainStorage;
            this.interviewViewRepository = interviewViewRepository;
            this.passwordHasher = passwordHasher;
        }

        public override async Task Synchronize(IProgress<SyncProgressInfo> progress,
            CancellationToken cancellationToken, SynchronizationStatistics statistics)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await this.UploadInterviewsAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.assignmentsSynchronizer.SynchronizeAssignmentsAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.SyncronizeSupervisor(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.SyncronizeInterviewers(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.SyncronizeCensusQuestionnaires(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.DownloadDeletedQuestionnairesListAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.CheckObsoleteQuestionnairesAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.DownloadInterviewsAsync(statistics, progress, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.techInfoSynchronizer.SynchronizeAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.logoSynchronizer.DownloadCompanyLogo(progress, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.auditLogSynchronizer.SynchronizeAuditLogAsync(progress, statistics, cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();
            await this.UpdateApplicationAsync(progress, cancellationToken);
        }

        private async Task DownloadDeletedQuestionnairesListAsync(IProgress<SyncProgressInfo> progress, SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Check_Obsolete_Questionnaires,
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });

            var deletedQuestionnairesList = await this.supervisorSyncService.GetListOfDeletedQuestionnairesIds(cancellationToken);

            this.deletedQuestionnairesStorage
                .Store(deletedQuestionnairesList.Select(id => new DeletedQuestionnaire{ Id = id }));
        }

        private async Task SyncronizeSupervisor(IProgress<SyncProgressInfo> progress, 
            SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_Supervisor_details,
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });

            var localSupervisor = this.supervisorsPlainStorage.FirstOrDefault();
            var supervisor = await this.supervisorSyncService.GetSupervisorAsync(token:cancellationToken);

            if (localSupervisor.Email != supervisor.Email)
            {
                localSupervisor.Email = supervisor.Email;
                this.supervisorsPlainStorage.Store(localSupervisor);

                principal.SignInWithHash(localSupervisor.Name, localSupervisor.PasswordHash, true);
            }
        }

        private ISupervisorSynchronizationService supervisorSyncService =>
            base.synchronizationService as ISupervisorSynchronizationService;

        private async Task SyncronizeInterviewers(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics, CancellationToken cancellationToken)
        {
            var processedInterviewersCount = 0;
            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_Interviewers,
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });

            var remoteInterviewers = await this.supervisorSyncService.GetInterviewersAsync(cancellationToken);
            var localInterviewers = this.interviewerViewRepository.LoadAll();

            var interviewersToRemove = localInterviewers.Select(x => x.InterviewerId)
                .Except(remoteInterviewers.Select(x => x.Id)).ToList();
            foreach (var interviewerId in interviewersToRemove)
            {
                this.interviewerViewRepository.Remove(interviewerId.FormatGuid());
            }

            processedInterviewersCount += interviewersToRemove.Count;
            var localInterviewersLookup = this.interviewerViewRepository.LoadAll().ToLookup(x => x.InterviewerId);

            foreach (var interviewer in remoteInterviewers)
            {
                var local = localInterviewersLookup[interviewer.Id].FirstOrDefault();
                if (local == null)
                {
                    local = new InterviewerDocument
                    {
                        Id = interviewer.Id.FormatGuid(),
                        InterviewerId = interviewer.Id,
                        CreationDate = interviewer.CreationDate,
                        Email = interviewer.Email,
                        PasswordHash = interviewer.PasswordHash,
                        PhoneNumber = interviewer.PhoneNumber,
                        UserName = interviewer.UserName,
                        FullaName = interviewer.FullName,
                        SecurityStamp = interviewer.SecurityStamp,
                        IsLockedBySupervisor = interviewer.IsLockedBySupervisor,
                        IsLockedByHeadquarters = interviewer.IsLockedByHeadquarters
                    };
                }
                else
                {
                    local.Email = interviewer.Email;
                    local.PasswordHash = interviewer.PasswordHash;
                    local.PhoneNumber = interviewer.PhoneNumber;
                    local.UserName = interviewer.UserName;
                    local.SecurityStamp = interviewer.SecurityStamp;
                    local.IsLockedBySupervisor = interviewer.IsLockedBySupervisor;
                    local.IsLockedByHeadquarters = interviewer.IsLockedByHeadquarters;
                }

                this.interviewerViewRepository.Store(local);

                processedInterviewersCount++;

                progress.Report(new SyncProgressInfo
                {
                    Title = InterviewerUIResources.Synchronization_Of_InterviewersFormat.FormatString(
                        processedInterviewersCount, remoteInterviewers.Count),
                    Statistics = statistics,
                    Status = SynchronizationStatus.Download
                });
            }

            progress.Report(new SyncProgressInfo
            {
                Title = InterviewerUIResources.Synchronization_Of_InterviewersFormat.FormatString(
                    processedInterviewersCount, remoteInterviewers.Count),
                Statistics = statistics,
                Status = SynchronizationStatus.Download
            });
        }


        protected override Task CheckAfterStartSynchronization(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        protected override void UpdatePasswordOfResponsible(RestCredentials credentials)
        {
            var localSupervisor = this.supervisorsPlainStorage.FirstOrDefault();
            localSupervisor.PasswordHash = this.passwordHasher.Hash(credentials.Password);
            localSupervisor.Token = credentials.Token;

            this.supervisorsPlainStorage.Store(localSupervisor);
            this.principal.SignIn(localSupervisor.Name, credentials.Password, true);
        }

        protected override int GetApplicationVersionCode()
        {
            return supervisorSettings.GetApplicationVersionCode();
        }

        protected override Task SyncronizeCensusQuestionnaires(IProgress<SyncProgressInfo> progress,
            SynchronizationStatistics statistics,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask; // supervisor does not support census
        }

        protected override Task<List<Guid>> FindObsoleteInterviewsAsync(IEnumerable<InterviewView> localInterviews,
            IEnumerable<InterviewApiView> remoteInterviews,
            IProgress<SyncProgressInfo> progress, CancellationToken cancellationToken)
        {
            return Task.FromResult(new List<Guid>());
        }

        protected override IReadOnlyCollection<InterviewView> GetInterviewsForUpload()
        {
            return this.interviewViewRepository.Where(interview =>
                interview.Status == InterviewStatus.ApprovedBySupervisor);
        }
    }
}