﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireDto;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_adding_roster_group_and_roster_size_question_is_under_deeper_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var anotherRosterId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
            groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(anotherRosterId,  chapterId, responsibleId: responsibleId);
            questionnaire.MarkGroupAsRoster(new GroupBecameARoster(responsibleId, anotherRosterId));
            questionnaire.AddNumericQuestion(rosterSizeQuestionId, isInteger : true, parentId: anotherRosterId,
                responsibleId:responsibleId);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.AddGroupAndMoveIfNeeded(groupId, responsibleId, "title",null, rosterSizeQuestionId, null, null, false, chapterId, true,
                    RosterSizeSourceType.Question, rosterFixedTitles: null, rosterTitleQuestionId: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__question_placed_deeper_then_roster = () =>
            new[] { "roster", "question", "deeper" }.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));

        private static Exception exception;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterSizeQuestionId;
        private static Questionnaire questionnaire;
        private static Guid chapterId;
    }
}
