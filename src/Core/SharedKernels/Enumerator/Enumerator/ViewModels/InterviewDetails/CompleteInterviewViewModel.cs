﻿using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class CompleteInterviewViewModel : MvxViewModel
    {
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly ICommandService commandService;
        private readonly IEntityWithErrorsViewModelFactory entityWithErrorsViewModelFactory;
        protected readonly IPrincipal principal;

        public InterviewStateViewModel InterviewState { get; set; }
        public DynamicTextViewModel Name { get; }

        public CompleteInterviewViewModel(
            IViewModelNavigationService viewModelNavigationService,
            ICommandService commandService,
            IPrincipal principal, 
            IMvxMessenger messenger,
            IEntityWithErrorsViewModelFactory entityWithErrorsViewModelFactory,
            InterviewStateViewModel interviewState,
            DynamicTextViewModel dynamicTextViewModel)
        {
            this.viewModelNavigationService = viewModelNavigationService;
            this.commandService = commandService;
            this.principal = principal;
            this.messenger = messenger;
            this.entityWithErrorsViewModelFactory = entityWithErrorsViewModelFactory;

            this.InterviewState = interviewState;
            this.Name = dynamicTextViewModel;
        }

        protected Guid interviewId;

        public virtual void Init(string interviewId,
            NavigationState navigationState)
        {
            this.interviewId = Guid.Parse(interviewId);

            this.InterviewState.Init(interviewId, null);
            this.Name.InitAsStatic(UIResources.Interview_Complete_Screen_Title);

            var questionsCount = InterviewState.QuestionsCount;
            this.AnsweredCount = InterviewState.AnsweredQuestionsCount;
            this.ErrorsCount = InterviewState.InvalidAnswersCount;
            this.UnansweredCount = questionsCount - this.AnsweredCount;

            this.EntitiesWithErrors =
                new ObservableCollection<EntityWithErrorsViewModel>(
                    entityWithErrorsViewModelFactory.GetEntities(interviewId, navigationState));
        }

        public int AnsweredCount { get; set; }

        public int UnansweredCount { get; set; }

        public int ErrorsCount { get; set; }

        public ObservableCollection<EntityWithErrorsViewModel> EntitiesWithErrors { get; private set; }

        private IMvxCommand completeInterviewCommand;
        public IMvxCommand CompleteInterviewCommand
        {
            get
            {
                return this.completeInterviewCommand ?? 
                    (this.completeInterviewCommand = new MvxCommand(async () => await this.CompleteInterviewAsync(), () => !wasThisInterviewCompleted));
            }
        }

        public string CompleteComment { get; set; }

        private bool wasThisInterviewCompleted = false;

        private async Task CompleteInterviewAsync()
        {
            this.wasThisInterviewCompleted = true;
            await this.commandService.WaitPendingCommandsAsync();

            var completeInterviewCommand = new CompleteInterviewCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                comment: this.CompleteComment,
                completeTime: DateTime.UtcNow);

            await this.commandService.ExecuteAsync(completeInterviewCommand);

            await this.CloseInterviewAsync();
        }

        protected virtual async Task CloseInterviewAsync()
        {
            await this.viewModelNavigationService.NavigateToDashboardAsync();

            this.messenger.Publish(new InterviewCompletedMessage(this));
        }
    }
}