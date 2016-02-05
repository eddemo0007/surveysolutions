﻿using System;
using System.Collections.Generic;
using System.Linq;
using MvvmCross.Platform.Core;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultiOptionLinkedQuestionViewModel : AbstractMultiOptionLinkedQuestionViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<AnswerRemoved>
    {
        private readonly AnswerNotifier answerNotifier;
        private readonly IAnswerToStringService answerToStringService;
        private Guid linkedToQuestionId;


        public MultiOptionLinkedQuestionViewModel(
            QuestionStateViewModel<MultipleOptionsLinkedQuestionAnswered> questionState,
            AnsweringViewModel answering,
            AnswerNotifier answerNotifier,
            IStatefulInterviewRepository interviewRepository,
            IAnswerToStringService answerToStringService,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage,
            IPrincipal userIdentity, ILiteEventRegistry eventRegistry,
            IMvxMainThreadDispatcher mainThreadDispatcher)
            : base(
                questionState, answering, interviewRepository, questionnaireStorage, userIdentity, eventRegistry,
                mainThreadDispatcher)
        {
            this.answerNotifier = answerNotifier;
            this.answerToStringService = answerToStringService;
        }

        protected override void InitFromModel(QuestionnaireModel questionnaire)
        {
            LinkedMultiOptionQuestionModel linkedQuestionModel =
                questionnaire.GetLinkedMultiOptionQuestion(questionIdentity.Id);
            this.maxAllowedAnswers = linkedQuestionModel.MaxAllowedAnswers;
            this.areAnswersOrdered = linkedQuestionModel.AreAnswersOrdered;
            this.linkedToQuestionId = linkedQuestionModel.LinkedToQuestionId;

            this.answerNotifier.Init(this.interviewId.FormatGuid(), this.linkedToQuestionId);
            this.answerNotifier.QuestionAnswered += this.LinkedToQuestionAnswered;
        }

        protected override IEnumerable<MultiOptionLinkedQuestionOptionViewModel> CreateOptions()
        {
            QuestionnaireModel questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);

            LinkedMultiOptionAnswer thisQuestionAnswers = interview.GetLinkedMultiOptionAnswer(this.questionIdentity);
            IEnumerable<BaseInterviewAnswer> linkedToQuestionAnswers =
                interview.FindAnswersOfReferencedQuestionForLinkedQuestion(this.linkedToQuestionId, this.questionIdentity);

            List<MultiOptionLinkedQuestionOptionViewModel> options = new List<MultiOptionLinkedQuestionOptionViewModel>();
            foreach (var answer in linkedToQuestionAnswers)
            {
                BaseQuestionModel linkedToQuestion = questionnaire.Questions[this.linkedToQuestionId];
                var option = this.BuildOption(questionnaire, linkedToQuestion, answer, thisQuestionAnswers);

                if (option != null)
                {
                    options.Add(option);
                }
            }
            return options;
        }

        public override void Dispose()
        {
            base.Dispose();
            this.answerNotifier.QuestionAnswered -= this.LinkedToQuestionAnswered;
        }

        private void LinkedToQuestionAnswered(object sender, EventArgs e)
        {
            MultiOptionLinkedQuestionOptionViewModel[] actualOptions = this.CreateOptions().ToArray();

            this.mainThreadDispatcher.RequestMainThreadAction(() => // otherwize its f.g magic with those observable collections. This is the only way I found to implement insertions without locks.
            {
                List<MultiOptionLinkedQuestionOptionViewModel> optionsToRemove = this
                    .Options
                    .Where(existingOption => !actualOptions.Any(actualOption => actualOption.Value.Identical(existingOption.Value)))
                    .ToList();

                foreach (var optionToRemove in optionsToRemove)
                {
                    this.Options.Remove(optionToRemove);
                }

                for (int actualOptionIndex = 0; actualOptionIndex < actualOptions.Length; actualOptionIndex++)
                {
                    var actualOption = actualOptions[actualOptionIndex];
                    var existingOption = this.Options.SingleOrDefault(option => option.Value.Identical(actualOption.Value));

                    if (existingOption != null)
                    {
                        existingOption.Title = actualOption.Title;
                    }
                    else
                    {
                        this.Options.Insert(actualOptionIndex, actualOption);
                    }
                }

                this.RaisePropertyChanged(() => this.HasOptions);
            });
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                RemoveOptionIfQuestionIsSourceofTheLink(question.Id, question.RosterVector);
            }
        }

        public void Handle(AnswerRemoved @event)
        {
            RemoveOptionIfQuestionIsSourceofTheLink(@event.QuestionId, @event.RosterVector);
        }

        private void RemoveOptionIfQuestionIsSourceofTheLink(Guid removedQuestionId,
            decimal[] removedQuestionRosterVector)
        {
            if (removedQuestionId != this.linkedToQuestionId)
                return;
            var shownAnswer = this.Options.SingleOrDefault(x => x.Value.SequenceEqual(removedQuestionRosterVector));
            if (shownAnswer != null)
            {
                this.InvokeOnMainThread(() => this.Options.Remove(shownAnswer));
                this.RaisePropertyChanged(() => this.HasOptions);
            }
        }

        private MultiOptionLinkedQuestionOptionViewModel BuildOption(QuestionnaireModel questionnaire,
            BaseQuestionModel linkedToQuestion,
            BaseInterviewAnswer linkedToAnswer,
            LinkedMultiOptionAnswer linkedMultiOptionAnswer)
        {
            var isChecked = linkedMultiOptionAnswer != null &&
                            linkedMultiOptionAnswer.IsAnswered &&
                            linkedMultiOptionAnswer.Answers.Any(x => x.Identical(linkedToAnswer.RosterVector));

            if (!linkedToAnswer.IsAnswered && !isChecked)
            {
                return null;
            }

            var title = this.BuildOptionTitle(questionnaire, linkedToQuestion, linkedToAnswer);

            var option = new MultiOptionLinkedQuestionOptionViewModel(this)
            {
                Title = title,
                Value = linkedToAnswer.RosterVector,
                Checked = isChecked,
                QuestionState = this.QuestionState
            };
            if (this.areAnswersOrdered && isChecked)
            {
                int selectedItemIndex = Array.FindIndex(linkedMultiOptionAnswer.Answers, x => x.Identical(linkedToAnswer.RosterVector)) + 1;
                option.CheckedOrder = selectedItemIndex;
            }

            return option;
        }

        private string BuildOptionTitle(QuestionnaireModel questionnaire, BaseQuestionModel linkedToQuestion, BaseInterviewAnswer linkedToAnswer)
        {
            string answerAsTitle = this.answerToStringService.AnswerToUIString(linkedToQuestion, linkedToAnswer, interview, questionnaire);

            int currentRosterLevel = this.questionIdentity.RosterVector.Length;

            IEnumerable<string> parentRosterTitlesWithoutLastOneAndFirstKnown =
                interview
                    .GetParentRosterTitlesWithoutLast(linkedToAnswer.Id, linkedToAnswer.RosterVector)
                    .Skip(currentRosterLevel);

            string rosterPrefixes = string.Join(": ", parentRosterTitlesWithoutLastOneAndFirstKnown);

            return string.IsNullOrEmpty(rosterPrefixes) ? answerAsTitle : string.Join(": ", rosterPrefixes, answerAsTitle);
        }
    }
}