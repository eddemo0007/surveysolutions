﻿using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class EntitiesListViewModelFactory : IEntitiesListViewModelFactory
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IInterviewViewModelFactory interviewViewModelFactory;
        private readonly IDynamicTextViewModelFactory dynamicTextViewModelFactory;

        private readonly int maxNumberOfEntities = 30;

        public EntitiesListViewModelFactory(
            IStatefulInterviewRepository interviewRepository, 
            IInterviewViewModelFactory interviewViewModelFactory, 
            IDynamicTextViewModelFactory dynamicTextViewModelFactory)
        {
            this.interviewRepository = interviewRepository;
            this.interviewViewModelFactory = interviewViewModelFactory;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
        }

        public IEnumerable<EntityWithErrorsViewModel> GetEntitiesWithErrors(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            Identity[] invalidEntities = interview.GetVisibleInvalidEntitiesInInterview().Take(this.maxNumberOfEntities).ToArray();
           
            return this.EntityWithErrorsViewModels<EntityWithErrorsViewModel>(interviewId, navigationState, invalidEntities, interview);
        }

        public IEnumerable<EntityWithCommentsViewModel> GetEntitiesWithComments(string interviewId, NavigationState navigationState)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            Identity[] commentedBySupervisorEntities = interview.GetCommentedBySupervisorQuestionsInInterview().Take(this.maxNumberOfEntities).ToArray();

            return this.EntityWithErrorsViewModels<EntityWithCommentsViewModel>(interviewId, navigationState, commentedBySupervisorEntities, interview);
        }

        private IEnumerable<T> EntityWithErrorsViewModels<T>(string interviewId, NavigationState navigationState,
            Identity[] invalidEntities, IStatefulInterview interview) where T : ListEntityViewModel
        {
            var entitiesWithErrors = new List<T>();
            foreach (var invalidEntity in invalidEntities)
            {
                var entityWithErrorsViewModel = this.interviewViewModelFactory.GetNew<T>();

                var navigationIdentity = NavigationIdentity.CreateForGroup(interview.GetParentGroup(invalidEntity),
                    invalidEntity);

                var title = this.dynamicTextViewModelFactory.CreateDynamicTextViewModel();

                title.Init(interviewId, navigationIdentity.AnchoredElementIdentity);

                entityWithErrorsViewModel.Init(navigationIdentity, title.PlainText, navigationState);
                entitiesWithErrors.Add(entityWithErrorsViewModel);
            }
            return entitiesWithErrors;
        }

        public int MaxNumberOfEntities => this.maxNumberOfEntities;
    }
}