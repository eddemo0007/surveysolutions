﻿using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Providers;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_linked_multiple_options_question_which_links_to_text_question_and_which_is_roster_title_for_2_rosters_and_roster_level_is_1 : InterviewTestsContext
    {
        Establish context = () =>
        {
            userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");

            emptyRosterVector = new decimal[] { };
            var rosterInstanceId = (decimal)22.5;
            rosterVector = emptyRosterVector.Concat(new[] { rosterInstanceId }).ToArray();

            questionId = Guid.Parse("11111111111111111111111111111111");
            var linkedToQuestionId = Guid.Parse("33333333333333333333333333333333");
            var linkedToRosterId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            rosterAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var linkedOption1Vector = new decimal[] { 0 };
            linkedOption2Vector = new decimal[] { 1 };
            linkedOption3Vector = new decimal[] { 2 };
            var linkedOption1Text = "linked option 1";
            linkedOption2Text = "linked option 2";
            linkedOption3Text = "linked option 3";


            var questionnaire = Mock.Of<IQuestionnaire>
            (_
                => _.HasQuestion(linkedToQuestionId) == true
                && _.GetQuestionType(linkedToQuestionId) == QuestionType.Text
                && _.GetRostersFromTopToSpecifiedQuestion(linkedToQuestionId) == new[] { linkedToRosterId }

                && _.HasQuestion(questionId) == true
                && _.GetQuestionType(questionId) == QuestionType.MultyOption
                && _.GetQuestionReferencedByLinkedQuestion(questionId) == linkedToQuestionId
                && _.GetRostersFromTopToSpecifiedQuestion(questionId) == new[] { rosterAId }
                && _.DoesQuestionSpecifyRosterTitle(questionId) == true
                && _.GetRostersAffectedByRosterTitleQuestion(questionId) == new[] { rosterAId, rosterBId }
            );


            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire));

            SetupInstanceToMockedServiceLocator<IInterviewExpressionStatePrototypeProvider>(
                CreateInterviewExpressionStateProviderStub());

            interview = CreateInterview(questionnaireId: questionnaireId);
            interview.Apply(Create.Events.RosterInstancesAdded(linkedToRosterId, emptyRosterVector, linkedOption1Vector[0], sortIndex: null));
            interview.Apply(Create.Events.RosterInstancesAdded(linkedToRosterId, emptyRosterVector, linkedOption2Vector[0], sortIndex: null));
            interview.Apply(Create.Events.RosterInstancesAdded(linkedToRosterId, emptyRosterVector, linkedOption3Vector[0], sortIndex: null));
            interview.Apply(new TextQuestionAnswered(userId, linkedToQuestionId, linkedOption1Vector, DateTime.Now, linkedOption1Text));
            interview.Apply(new TextQuestionAnswered(userId, linkedToQuestionId, linkedOption2Vector, DateTime.Now, linkedOption2Text));
            interview.Apply(new TextQuestionAnswered(userId, linkedToQuestionId, linkedOption3Vector, DateTime.Now, linkedOption3Text));
            interview.Apply(Create.Events.RosterInstancesAdded(rosterAId, emptyRosterVector, rosterInstanceId, sortIndex: null));
            interview.Apply(Create.Events.RosterInstancesAdded(rosterBId, emptyRosterVector, rosterInstanceId, sortIndex: null));

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerMultipleOptionsLinkedQuestion(userId, questionId, rosterVector, DateTime.Now,
                new [] { linkedOption3Vector, linkedOption2Vector });

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_MultipleOptionsLinkedQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<MultipleOptionsLinkedQuestionAnswered>();

        It should_raise_1_RosterRowsTitleChanged_events = () =>
            eventContext.ShouldContainEvents<RosterInstancesTitleChanged>(count: 1);

        It should_set_2_affected_roster_ids_in_RosterRowsTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(r => r.RosterInstance.GroupId)).ToArray()
                .ShouldContainOnly(rosterAId, rosterBId);

        It should_set_empty_outer_roster_vector_to_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.OuterRosterVector.SequenceEqual(emptyRosterVector)));

        It should_set_last_element_of_roster_vector_to_roster_instance_id_in_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>()
                .ShouldEachConformTo(@event => @event.ChangedInstances.All(x => x.RosterInstance.RosterInstanceId == rosterVector.Last()));

        It should_set_title_to_text_assigned_to_corresponding_linked_to_questions_separated_by_comma_and_space_in_answer_order_in_all_RosterRowTitleChanged_events = () =>
            eventContext.GetEvents<RosterInstancesTitleChanged>().SelectMany(@event => @event.ChangedInstances.Select(x => x.Title))
                .ShouldEachConformTo(title => title == linkedOption3Text + ", " + linkedOption2Text);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid questionId;
        private static decimal[] rosterVector;
        private static decimal[] emptyRosterVector;
        private static Guid rosterAId;
        private static Guid rosterBId;
        private static decimal[] linkedOption2Vector;
        private static string linkedOption2Text;
        private static decimal[] linkedOption3Vector;
        private static string linkedOption3Text;
    }
}