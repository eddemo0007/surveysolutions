using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.MultiOptionLinkedQuestionViewModelTests
{
    internal class when_questions_enabled_event_received_by_linked_question : MultiOptionLinkedQuestionViewModelTestsContext
    {
        Establish context = () =>
        {
            var interview = new Mock<IStatefulInterview>();

            interview.Setup(x => x.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedToQuestionId, Moq.It.IsAny<Identity>()))
                .Returns(new[]
                {
                    Create.TextAnswer("answer2", linkedToQuestionId, Create.RosterVector(2))
                });

            interview.Setup(x =>x.Answers)
                .Returns(new Dictionary<string, BaseInterviewAnswer>());

            var questionnaire = Mock.Of<IQuestionnaire>(_
                => _.GetQuestionReferencedByLinkedQuestion(questionId.Id) == linkedToQuestionId
                && _.ShouldQuestionRecordAnswersOrder(questionId.Id) == false);

            var questionnaires = new Mock<IPlainQuestionnaireRepository>();
            questionnaires.SetReturnsDefault(questionnaire);

            var interviews = new Mock<IStatefulInterviewRepository>();
            interviews.SetReturnsDefault(interview.Object);

            questionViewModel = CreateViewModel(interviewRepository: interviews.Object, questionnaireStorage: questionnaires.Object);
            questionViewModel.Init("interviewId", questionId, Create.NavigationState());

            interview.Setup(x => x.FindAnswersOfReferencedQuestionForLinkedQuestion(linkedToQuestionId, Moq.It.IsAny<Identity>()))
                .Returns(new[]
                {
                    Create.TextAnswer("answer1", linkedToQuestionId, Create.RosterVector(1)),
                    Create.TextAnswer("answer2", linkedToQuestionId, Create.RosterVector(2))
                });
        };

        Because of = () =>
        {
            questionViewModel.Handle(Create.Event.QuestionsEnabled(linkedToQuestionId, Create.RosterVector(1)));
        };

        It should_decrease_amount_of_options = () =>
            questionViewModel.Options.Count.ShouldEqual(2);

        It should_have_option_1_with_roster_code__1 = () =>
            questionViewModel.Options.First().Value.ShouldContainOnly(Create.RosterVector(1));

        It should_have_option_2_with_roster_code__2 = () =>
            questionViewModel.Options.Second().Value.ShouldContainOnly(Create.RosterVector(2));

        private static MultiOptionLinkedToQuestionQuestionViewModel questionViewModel;
        private static readonly Guid linkedToQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static readonly Identity questionId = Create.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), Empty.RosterVector);
    }
}