﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_and_title_contains_undefined_substitution : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(Create.Event.AddGroup(groupId: chapterId));
            questionnaire.Apply(Create.Event.AddTextQuestion(questionId: textQuestionId, parentId: chapterId));
            questionnaire.Apply(Create.Event.AddGroup(groupId: groupId, parentId:chapterId));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => exception =
            Catch.Exception(() => questionnaire.UpdateGroup(
                groupId: groupId,
                responsibleId: responsibleId,
                title: $"title %{textQuestionVariable}%",
                variableName: null,
                rosterSizeQuestionId: null,
                description: null,
                condition: null,
                hideIfDisabled: false,
                isRoster: false,
                rosterSizeSource: RosterSizeSourceType.Question,
                rosterFixedTitles: null,
                rosterTitleQuestionId: null));

        It should_exception_has_specified_message = () =>
            new[] {"unknown", "substitution", textQuestionVariable}.ShouldEachConformTo(x =>
                ((QuestionnaireException) exception).Message.Contains(x));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid groupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid textQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static string textQuestionVariable = "hhname";
        private static Exception exception;
    }
}