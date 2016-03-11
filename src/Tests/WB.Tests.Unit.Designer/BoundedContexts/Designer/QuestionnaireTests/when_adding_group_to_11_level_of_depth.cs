﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_adding_group_to_11_level_of_depth : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            responsibleId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaireWithNesingAndLastGroup(10, parentGroupId, responsibleId);
        };

        Because of = () =>
            exception = Catch.Exception(
                () =>
                    questionnaire.AddGroupAndMoveIfNeeded(groupId,
                responsibleId: responsibleId, title: "New group", variableName: null, rosterSizeQuestionId: null, description: null,
                condition: null, hideIfDisabled: false, parentGroupId: parentGroupId, isRoster: false, rosterSizeSource: RosterSizeSourceType.Question,
                rosterFixedTitles: null,
                rosterTitleQuestionId: null));

                    

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message = () =>
            new[] { "sub-section", "roster", "depth", "higher", "10"}.ShouldEachConformTo(keyword => exception.Message.ToLower().Contains(keyword));
        

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid parentGroupId;
        
        private static Exception exception;
    }
}