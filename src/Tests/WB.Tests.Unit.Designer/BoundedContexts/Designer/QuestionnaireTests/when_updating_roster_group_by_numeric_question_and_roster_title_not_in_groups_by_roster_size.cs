﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_updating_roster_group_by_numeric_question_and_roster_title_not_in_groups_by_roster_size : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            var chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            groupId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterTitleQuestionId = Guid.Parse("1BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeQuestionId = Guid.Parse("2BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            rosterSizeSourceType = RosterSizeSourceType.Question;

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NewGroupAdded { PublicKey = groupId, ParentGroupPublicKey = chapterId });
            questionnaire.Apply(Create.Event.NewQuestionAdded(
                publicKey : rosterTitleQuestionId,
                groupPublicKey : chapterId,
                questionType : QuestionType.Text
            ));
            questionnaire.Apply(Create.Event.NewQuestionAdded(
                publicKey : rosterSizeQuestionId,
                groupPublicKey : chapterId,
                questionType : QuestionType.Numeric,
                isInteger : true
            ));
            questionnaire.Apply(new NewGroupAdded { PublicKey = parentGroupId });
        };

        Because of = () =>
            exception = Catch.Exception(
                () =>
                    questionnaire.UpdateGroup(groupId: groupId, responsibleId: responsibleId, title: "title", variableName: null,
                        description: null, condition: null, hideIfDisabled: false, rosterSizeQuestionId: rosterSizeQuestionId,
                        isRoster: true, rosterSizeSource: rosterSizeSourceType, rosterFixedTitles: null,
                        rosterTitleQuestionId: rosterTitleQuestionId));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__roster__ = () =>
            exception.Message.ToLower().ShouldContain("roster");

        It should_throw_exception_with_message_containting__title__ = () =>
            exception.Message.ToLower().ShouldContain("title");

        It should_throw_exception_with_message_containting__question__ = () =>
            exception.Message.ToLower().ShouldContain("question");

        It should_throw_exception_with_message_containting__only__ = () =>
            exception.Message.ToLower().ShouldContain("only");

        It should_throw_exception_with_message_containting__inside__ = () =>
            exception.Message.ToLower().ShouldContain("inside");

        It should_throw_exception_with_message_containting__group__ = () =>
            exception.Message.ToLower().ShouldContain("group");

        It should_throw_exception_with_message_containting__where__ = () =>
            exception.Message.ToLower().ShouldContain("where");

        It should_throw_exception_with_message_containting__size__ = () =>
            exception.Message.ToLower().ShouldContain("source");

        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid groupId;
        private static Guid rosterTitleQuestionId;
        private static Guid rosterSizeQuestionId;
        private static Guid parentGroupId;
        private static RosterSizeSourceType rosterSizeSourceType;
        private static Exception exception;
    }
}