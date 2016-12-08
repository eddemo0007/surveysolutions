﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedToListQuestionQuestionViewModel : MvxNotifyPropertyChanged,
        IMultiOptionQuestionViewModelToggleable,
        IInterviewEntityViewModel,
        ILiteEventHandler<TextListQuestionAnswered>,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<LinkedToListOptionsChanged>,
        ILiteEventHandler<MultipleOptionsQuestionAnswered>,
        ILiteEventHandler<QuestionsEnabled>,
        ILiteEventHandler<QuestionsDisabled>,
        ICompositeQuestionWithChildren,
        IDisposable
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly ILiteEventRegistry eventRegistry;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IPrincipal userIdentity;

        protected readonly IMvxMainThreadDispatcher mainThreadDispatcher;
        private readonly QuestionInstructionViewModel instructionViewModel;
        private readonly QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionState;

        protected IStatefulInterview interview;
        private Guid interviewId;
        private Identity questionIdentity;
        private Guid userId;
        private Guid linkedToQuestionId;
        private int? maxAllowedAnswers;
        private bool areAnswersOrdered;

        public QuestionInstructionViewModel InstructionViewModel => this.instructionViewModel;
        public IQuestionStateViewModel QuestionState => this.questionState;
        public AnsweringViewModel Answering { get; private set; }

        public CovariantObservableCollection<MultiOptionQuestionOptionViewModel> options { get; private set; }

        private OptionBorderViewModel<MultipleOptionsQuestionAnswered> optionsTopBorderViewModel;
        private OptionBorderViewModel<MultipleOptionsQuestionAnswered> optionsBottomBorderViewModel;

        public CovariantObservableCollection<MultiOptionQuestionOptionViewModel> Options
        {
            get { return this.options; }
            private set { this.options = value; this.RaisePropertyChanged(() => this.HasOptions); }
        }

        public bool HasOptions => this.Options.Any();

        public MultiOptionLinkedToListQuestionQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsQuestionAnswered> questionState,
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel,
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            IPrincipal userIdentity, ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
        {
            this.Options = new CovariantObservableCollection<MultiOptionQuestionOptionViewModel>();
            this.questionState = questionState;
            this.questionnaireStorage = questionnaireStorage;
            this.eventRegistry = eventRegistry;
            this.userIdentity = userIdentity;
            this.instructionViewModel = instructionViewModel;
            this.interviewRepository = interviewRepository;
            this.Answering = answering;
            this.mainThreadDispatcher = mainThreadDispatcher;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.eventRegistry.Subscribe(this, interviewId);
            this.questionIdentity = entityIdentity;
            this.userId = this.userIdentity.CurrentUserIdentity.UserId;
            this.interview = this.interviewRepository.Get(interviewId);
            this.interviewId = this.interview.Id;
            this.InitFromModel(this.questionnaireStorage.GetQuestionnaire(this.interview.QuestionnaireIdentity, this.interview.Language));

            this.instructionViewModel.Init(interviewId, entityIdentity);
            
            this.options.CollectionChanged += (sender, args) =>
            {
                if (this.optionsTopBorderViewModel != null)
                {
                    this.optionsTopBorderViewModel.HasOptions = HasOptions;
                }
                if (this.optionsBottomBorderViewModel != null)
                {
                    this.optionsBottomBorderViewModel.HasOptions = this.HasOptions;
                }
            };
            this.RefreshOptionsFromModel();
        }


        protected void InitFromModel(IQuestionnaire questionnaire)
        {
            this.maxAllowedAnswers = questionnaire.GetMaxSelectedAnswerOptions(questionIdentity.Id);
            this.areAnswersOrdered = questionnaire.ShouldQuestionRecordAnswersOrder(questionIdentity.Id);
            this.linkedToQuestionId = questionnaire.GetQuestionReferencedByLinkedQuestion(questionIdentity.Id);
        }

        public async Task ToggleAnswerAsync(MultiOptionQuestionOptionViewModelBase changedModel)
        {
            List<MultiOptionQuestionOptionViewModel> allSelectedOptions =
                this.areAnswersOrdered ?
                this.Options.Where(x => x.Checked).OrderBy(x => x.CheckedTimeStamp).ThenBy(x => x.CheckedOrder ?? 0).ToList() :
                this.Options.Where(x => x.Checked).ToList();

            if (this.maxAllowedAnswers.HasValue && allSelectedOptions.Count > this.maxAllowedAnswers)
            {
                changedModel.Checked = false;
                return;
            }

            
            var selectedValues = allSelectedOptions.Select(x => x.Value).ToArray();

            var command = new AnswerMultipleOptionsQuestionCommand(
                this.interviewId,
                this.userId,
                this.questionIdentity.Id,
                this.questionIdentity.RosterVector,
                DateTime.UtcNow,
                selectedValues);

            try
            {
                await this.Answering.SendAnswerQuestionCommandAsync(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                changedModel.Checked = !changedModel.Checked;
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        public void Dispose()
        {
            this.questionState.Dispose();
            this.eventRegistry.Unsubscribe(this);
        }

        public IObservableCollection<ICompositeEntity> Children
        {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                this.optionsTopBorderViewModel = new OptionBorderViewModel<MultipleOptionsQuestionAnswered>(this.questionState, true)
                {
                    HasOptions = HasOptions
                };
                result.Add(this.optionsTopBorderViewModel);
                result.AddCollection(this.Options);
                this.optionsBottomBorderViewModel = new OptionBorderViewModel<MultipleOptionsQuestionAnswered>(this.questionState, false)
                {
                    HasOptions = HasOptions
                };
                result.Add(this.optionsBottomBorderViewModel);
                return result;
            }
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId)) return;

            this.RefreshOptionsFromModel();
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.All(x => x.Id != this.linkedToQuestionId))
                return;

            this.ClearOptions();
        }

        public void Handle(AnswersRemoved @event)
        {
            if (@event.Questions.Contains(this.Identity))
            {
                foreach (var option in this.options)
                    this.UpdateOptionSelection(option, new List<decimal>());
            }

            if (@event.Questions.Any(question => question.Id == this.linkedToQuestionId))
                this.ClearOptions();
        }

        public void Handle(MultipleOptionsQuestionAnswered @event)
        {
            if (!this.areAnswersOrdered) return;

            if (@event.QuestionId != this.questionIdentity.Id || !@event.RosterVector.Identical(this.questionIdentity.RosterVector))
                return;

            foreach (var option in this.options)
                this.UpdateOptionSelection(option, @event.SelectedValues.ToList());
        }

        private void UpdateOptionSelection(MultiOptionQuestionOptionViewModel option, List<decimal> selectedOptionValues)
        {
            option.Checked = selectedOptionValues.Contains(option.Value);

            if (!this.areAnswersOrdered) return;

            var orderNo = selectedOptionValues.IndexOf(option.Value);

            option.CheckedOrder = orderNo == -1 ? (int?) null : orderNo + 1;
        }

        public void Handle(TextListQuestionAnswered @event)
        {
            if (@event.QuestionId != this.linkedToQuestionId)
                return;

            foreach (var answer in @event.Answers)
            {
                var option = this.options.FirstOrDefault(o => o.Value == answer.Item1);

                if (option != null) option.Title = answer.Item2;
            }
        }

        public void Handle(LinkedToListOptionsChanged @event)
        {
            if (@event.ChangedLinkedQuestions.All(x => x.QuestionId != this.Identity)) return;

            this.RefreshOptionsFromModel();
        }

        private void RefreshOptionsFromModel()
        {
            var textListAnswerRows = this.GetTextListAnswerRows();
            this.mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                this.RemoveOptions(textListAnswerRows);
                this.InsertOrUpdateOptions(textListAnswerRows);

                this.RaisePropertyChanged(() => this.HasOptions);
            });
        }

        private void InsertOrUpdateOptions(List<TextListAnswerRow> textListAnswerRows)
        {
            var linkedQuestionAnswer = interview.GetMultiOptionLinkedToListQuestion(this.Identity).GetAnswer()?.CheckedValues?
                .Select(optionValue => (decimal) optionValue)?
                .ToList() ?? new List<decimal>();

            foreach (var textListAnswerRow in textListAnswerRows)
            {
                var viewModelOption = this.options.FirstOrDefault(o => o.Value == textListAnswerRow.Value);
                if (viewModelOption == null)
                {
                    viewModelOption = this.CreateOptionViewModel(textListAnswerRow);
                    this.options.Insert(textListAnswerRows.IndexOf(textListAnswerRow), viewModelOption);
                }

                this.UpdateOptionSelection(viewModelOption, linkedQuestionAnswer);
            }
        }

        private void RemoveOptions(List<TextListAnswerRow> textListAnswerRows)
        {
            var removedOptionValues =
                this.options.Select(option => option.Value).Except(textListAnswerRows.Select(row => (int)row.Value)).ToList();

            foreach (var removedOptionValue in removedOptionValues)
            {
                                var removedOption =
                                    this.options.SingleOrDefault(option => option.Value == removedOptionValue);
                                if (removedOption == null) continue;

                                this.options.Remove(removedOption);
            }
        }

        private List<TextListAnswerRow> GetTextListAnswerRows()
        {
            var listQuestion = interview.FindQuestionInQuestionBranch(this.linkedToQuestionId, this.Identity);

            if ((listQuestion == null) || listQuestion.IsDisabled() || listQuestion.AsTextList?.GetAnswer()?.Rows == null)
                return new List<TextListAnswerRow>();

            return new List<TextListAnswerRow>(listQuestion.AsTextList.GetAnswer().Rows);
        }

        private void ClearOptions() =>
            this.mainThreadDispatcher.RequestMainThreadAction(() =>
            {
                this.options.Clear();
                this.RaisePropertyChanged(() => this.HasOptions);
            });

        private MultiOptionQuestionOptionViewModel CreateOptionViewModel(TextListAnswerRow optionValue)
            => new MultiOptionQuestionOptionViewModel(this)
            {
                Title = optionValue.Text,
                Value = Convert.ToInt32(optionValue.Value),
                QuestionState = this.questionState
            };
    }
}