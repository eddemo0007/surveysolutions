﻿using System;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.YesNoQuestionViewModelTests
{
    internal class when_initializing_answered_question : YesNoQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            interviewId = "interview";
            questionGuid = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            questionId = Create.Identity(questionGuid, Empty.RosterVector);
            navigationState = Create.NavigationState();

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.ShouldQuestionRecordAnswersOrder(questionId.Id) == true
                && _.GetMaxSelectedAnswerOptions(questionId.Id) == null
                && _.ShouldQuestionSpecifyRosterSize(questionId.Id) == false
                && _.GetAnswerOptionsAsValues(questionId.Id) == new decimal[] { 1, 2, 3, 4, 5 }
                && _.GetAnswerOptionTitle(questionId.Id, 1) == "item1"
                && _.GetAnswerOptionTitle(questionId.Id, 2) == "item2"
                && _.GetAnswerOptionTitle(questionId.Id, 3) == "item3"
                && _.GetAnswerOptionTitle(questionId.Id, 4) == "item4"
                && _.GetAnswerOptionTitle(questionId.Id, 5) == "item5"
            );

            var yesNoAnswer = Create.YesNoAnswer(questionGuid, Empty.RosterVector);
            yesNoAnswer.SetAnswers(new[]
            {
                new AnsweredYesNoOption(5, true),
                new AnsweredYesNoOption(2, false),
            });

            var interview = Mock.Of<IStatefulInterview>(x => x.GetYesNoAnswer(questionId) == yesNoAnswer);

            var questionnaireStorage = new Mock<IPlainQuestionnaireRepository>();
            var interviewRepository = new Mock<IStatefulInterviewRepository>();

            questionnaireStorage.SetReturnsDefault(questionnaire);
            interviewRepository.SetReturnsDefault(interview);

            eventRegistry = new Mock<ILiteEventRegistry>();

            viewModel = CreateViewModel(questionnaireStorage: questionnaireStorage.Object, 
                interviewRepository: interviewRepository.Object,
                eventRegistry: eventRegistry.Object);
        };

        Because of = () => viewModel.InitAsync(interviewId, questionId, navigationState);

        It should_build_options = () => viewModel.Options.Count.ShouldEqual(5);

        It should_mark_answered_options_as_checked = () =>
        {
            var lastOption = viewModel.Options.Last();
            lastOption.YesSelected.ShouldBeTrue();
            lastOption.Title.ShouldEqual("item5");
            lastOption.YesAnswerCheckedOrder.ShouldEqual(1);
            lastOption.Value.ShouldEqual(5m);
        };

        It should_subscribe_model_in_events_registry = () => eventRegistry.Verify(x => x.Subscribe(viewModel, Moq.It.IsAny<string>()));

        static YesNoQuestionViewModel viewModel;
        static string interviewId;
        static Identity questionId;
        static NavigationState navigationState;
        private static Guid questionGuid;
        private static Mock<ILiteEventRegistry> eventRegistry;
    }
}

