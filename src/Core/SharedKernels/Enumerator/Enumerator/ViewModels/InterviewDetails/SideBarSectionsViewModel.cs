using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails
{
    public class SideBarSectionsViewModel : MvxNotifyPropertyChanged,
        IAsyncViewModelEventHandler<GroupsEnabled>,
        IAsyncViewModelEventHandler<GroupsDisabled>,
        IAsyncViewModelEventHandler<RosterInstancesRemoved>,
        IDisposable
    {
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher;
        private NavigationState navigationState;

        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly ISideBarSectionViewModelsFactory modelsFactory;
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private string interviewId;
        private bool needAddVirtualCoverPage = false;
        private List<Identity> sectionIdentities;

        public SynchronizedList<ISideBarSectionItem> items = new SynchronizedList<ISideBarSectionItem>();

        private ObservableRangeCollection<ISideBarItem> allVisibleSections;
        public ObservableRangeCollection<ISideBarItem> AllVisibleSections
        {
            get { return this.allVisibleSections; }
            set { this.RaiseAndSetIfChanged(ref this.allVisibleSections, value); }
        }

        public SideBarSectionsViewModel(
            IStatefulInterviewRepository statefulInterviewRepository,
            IQuestionnaireStorage questionnaireRepository,
            ISideBarSectionViewModelsFactory modelsFactory,
            IViewModelEventRegistry eventRegistry,
            IMvxMainThreadAsyncDispatcher mainThreadAsyncDispatcher)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.modelsFactory = modelsFactory;
            this.statefulInterviewRepository = statefulInterviewRepository;
            this.eventRegistry = eventRegistry;
            this.mainThreadAsyncDispatcher = mainThreadAsyncDispatcher;
        }

        public void Init(string interviewId, NavigationState navigationState)
        {
            if (this.navigationState != null) throw new Exception("ViewModel already initialized");
            this.interviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
            this.navigationState = navigationState ?? throw new ArgumentNullException(nameof(navigationState));

            this.navigationState.ScreenChanged += this.OnScreenChanged;

            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            
            this.sectionIdentities = questionnaire.GetAllSections()
                .Select(sectionId => Identity.Create(sectionId, RosterVector.Empty))
                .ToList();

            needAddVirtualCoverPage = !questionnaire.IsCoverPageSupported;
            var predefItems = new List<ISideBarItem>();
            
            if (needAddVirtualCoverPage)
                predefItems.Add(this.modelsFactory.BuildVirtualCoverItem(this.navigationState));
            predefItems.Add(this.modelsFactory.BuildOverviewItem(this.navigationState, this.interviewId));
            predefItems.Add(this.modelsFactory.BuildCompleteItem(this.navigationState, this.interviewId));

            this.AllVisibleSections = new ObservableRangeCollection<ISideBarItem>(predefItems);
            this.UpdateSectionsAsync().WaitAndUnwrapException();

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private async void OnScreenChanged(ScreenChangedEventArgs e) => await this.UpdateSectionsAsync(clearExpanded: true);

        public async Task HandleAsync(GroupsEnabled @event)
        {
            var addedSections = @event.Groups.Intersect(this.sectionIdentities).ToArray();

            if (addedSections.Any())
                await this.UpdateSectionsAsync();
        }

        public async Task HandleAsync(GroupsDisabled @event)
        {
            var removedViewModels = this.items.Where(
                    sectionViewModel => @event.Groups.Contains(sectionViewModel.SectionIdentity)).ToList();

            if (removedViewModels.Any())
                await this.UpdateSectionsAsync();
        }

        public async Task HandleAsync(RosterInstancesRemoved @event)
        {
            var removedViewModels = this.items.Where(sectionViewModel =>
                    @event.Instances.Select(x => x.GetIdentity()).Contains(sectionViewModel.SectionIdentity)).ToList();

            if (removedViewModels.Any())
                await this.UpdateSectionsAsync();
        }

        private async void UpdateSectionsAsync(object sender, EventArgs e)
        {
            var args = e as ToggleSectionEventArgs;

            await this.UpdateSectionsAsync(args, false);
        }

        private Task UpdateSectionsAsync(ToggleSectionEventArgs toggledSection = null, bool clearExpanded = false)
        {
            var expectedSectionIdentities = this.GetSectionsAndExpandedSubSections(clearExpanded, toggledSection).ToList();
            this.UpdateViewModels(expectedSectionIdentities);
            return this.UpdateUIAsync();
        }

        private void UpdateViewModels(List<Identity> expectedSectionIdentities)
        {
            var viewModelIdentities = this.items.Select(viewModel => viewModel.SectionIdentity).ToArray();

            var notChangedSectionIdentities = expectedSectionIdentities.Intersect(viewModelIdentities).ToArray();

            var removedViewModelIdentities = viewModelIdentities.Except(notChangedSectionIdentities).ToArray();
            var addedViewModelIdentities = expectedSectionIdentities.Except(notChangedSectionIdentities).ToArray();

            foreach (var identity in removedViewModelIdentities)
            {
                var removedViewModel = this.items.FirstOrDefault(viewModel => viewModel.SectionIdentity == identity);
                if (removedViewModel == null) continue;

                this.DisposeSectionViewModel(removedViewModel);
                this.items.Remove(removedViewModel);
            }

            foreach (var identity in addedViewModelIdentities)
            {
                if (this.isDisposed) return;
                this.items.Insert(expectedSectionIdentities.IndexOf(identity), this.CreateSideBarSectionItem(identity));
            }
        }

        private Task UpdateUIAsync() => mainThreadAsyncDispatcher.ExecuteOnMainThreadAsync(() =>
        {
            var sectionItems = this.AllVisibleSections.OfType<ISideBarSectionItem>().ToArray();

            var notChangedItems = sectionItems.Intersect(this.items).ToArray();
            var removedViewModels = sectionItems.Except(notChangedItems).ToArray();
            var addedSectionViewModels = this.items.Except(notChangedItems);

            this.AllVisibleSections.RemoveRange(removedViewModels);

            var offset = needAddVirtualCoverPage ? 1 : 0;
            foreach (var addedSectionViewModel in addedSectionViewModels)
                this.AllVisibleSections.Insert(this.items.IndexOf(addedSectionViewModel) + offset, addedSectionViewModel);
        });

        internal IEnumerable<Identity> GetSectionsAndExpandedSubSections(bool clearExpanded, ToggleSectionEventArgs toggledSection = null)
        {
            var interview = this.statefulInterviewRepository.Get(this.interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            bool IsSectionVisible(InterviewTreeGroup group)
            {
                if (questionnaire.IsCoverPage(group.Identity.Id))
                {
                    return !string.IsNullOrWhiteSpace(interview.GetLastSupervisorComment())
                           || interview.GetCommentedBySupervisorQuestionsVisibleToInterviewer().Any()
                           || questionnaire.GetPrefilledQuestions().Any();
                }
                
                return !@group.IsDisabled() 
                       || @group.IsDisabled() && !questionnaire.ShouldBeHiddenIfDisabled(@group.Identity.Id);
            }

            List<Identity> expandedSectionIdentities = CollectAllExpandedUiSections().ToList();
            var currentGroup = interview.GetGroup(this.navigationState.CurrentGroup);
            List<Identity> parentsOfCurrentGroup = GetCurrentSectionAndItsParentsIdentities(interview, questionnaire, this.navigationState.CurrentGroup);

            List<Identity> itemsToBeExpanded = expandedSectionIdentities.Union(parentsOfCurrentGroup).Distinct().ToList();

            var isSomeSectionBeingCollapled = toggledSection != null && !toggledSection.IsExpandedNow;
            if (isSomeSectionBeingCollapled)
            {
                RemoveItemsUnderSectionBeingCollapsed(itemsToBeExpanded, toggledSection.ToggledSection, interview);
            }

            var itemsToBeExpandedAndTheirImmidiateChildren = new HashSet<Identity>();
            foreach (var identity in itemsToBeExpanded)
            {
                itemsToBeExpandedAndTheirImmidiateChildren.Add(identity);
                interview.GetGroup(identity)?.GetAllSubGroups().Where(IsSectionVisible)
                    .ForEach(x => itemsToBeExpandedAndTheirImmidiateChildren.Add(x.Identity));
            }

            var sectionOrSubSections = interview.GetAllGroupsAndRosters().Where(x =>
                IsSectionVisible(x) && !questionnaire.IsFlatRoster(x.Identity.Id));
            
            foreach (var sectionOrSubSection in  sectionOrSubSections)
            {
                if (sectionOrSubSection is InterviewTreeSection)
                    yield return sectionOrSubSection.Identity;

                if (sectionOrSubSection.Parent == null) continue;

                var isInCurrentSection = sectionOrSubSection.Parent.Identity == currentGroup?.Identity;
                var isParentOfCurrentSection = currentGroup != null && parentsOfCurrentGroup.Contains(sectionOrSubSection.Parent.Identity);
                var isExpandedSection = itemsToBeExpandedAndTheirImmidiateChildren.Contains(sectionOrSubSection.Identity);

                if (clearExpanded && (isParentOfCurrentSection || isInCurrentSection))
                    yield return sectionOrSubSection.Identity;

                if (!clearExpanded && isExpandedSection)
                    yield return sectionOrSubSection.Identity;
            }
        }

        private List<Identity> GetCurrentSectionAndItsParentsIdentities(IStatefulInterview interview, IQuestionnaire questionnaire, Identity currentGroupIdentity)
        {
            var currentGroup = interview.GetGroup(currentGroupIdentity) 
                               ?? interview.GetEnabledSections().First();
            
            List<Identity> parentsOfCurrentGroup = new List<Identity>{ currentGroup.Identity };
            parentsOfCurrentGroup.AddRange(currentGroup.Parents?.Select(group => @group.Identity));
            return parentsOfCurrentGroup;
        }

        private IEnumerable<Identity> CollectAllExpandedUiSections()
        {
            foreach (var sectionItem in this.items)
            {
                if (!sectionItem.Expanded) continue;
                yield return sectionItem.SectionIdentity;
            }
        }

        private static void RemoveItemsUnderSectionBeingCollapsed(
            List<Identity> itemsToBeEpanded,
            Identity sectionBeingCollapsed,
            IStatefulInterview interview)
        {
            for (int i = itemsToBeEpanded.Count - 1; i >= 0; i--)
            {
                if (itemsToBeEpanded[i] == sectionBeingCollapsed)
                    itemsToBeEpanded.RemoveAt(i);
                else if (interview.IsParentOf(sectionBeingCollapsed, itemsToBeEpanded[i]))
                    itemsToBeEpanded.RemoveAt(i);
            }
        }

        private ISideBarSectionItem CreateSideBarSectionItem(Identity sectionIdentity)
        {
            var sectionViewModel = this.modelsFactory.BuildSectionItem(sectionIdentity, this.navigationState,
                this.interviewId);

            sectionViewModel.OnSectionUpdated += this.UpdateSectionsAsync;
            return sectionViewModel;
        }

        private bool isDisposed;
        public void Dispose()
        {
            if (!this.isDisposed)
            {
                this.isDisposed = true;

                if(this.navigationState!= null)
                    this.navigationState.ScreenChanged -= this.OnScreenChanged;

                if(this.eventRegistry != null)
                    this.eventRegistry.Unsubscribe(this);
                
                this.AllVisibleSections?.ForEach(viewModel =>
                {
                    var sectionViewModel = viewModel as ISideBarSectionItem;
                    if (sectionViewModel != null)
                        this.DisposeSectionViewModel(sectionViewModel);
                    else
                        viewModel?.Dispose();
                });
                var itemsToDispose = this.items;
                foreach (var sideBarSectionItem in itemsToDispose)
                {
                    sideBarSectionItem?.DisposeIfDisposable();
                }
            }
        }

        private void DisposeSectionViewModel(ISideBarSectionItem viewModel)
        {
            viewModel.OnSectionUpdated -= this.UpdateSectionsAsync;
            viewModel.Dispose();
        }
    }
}
