﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireTests;

namespace WB.Tests.Unit.BoundedContexts.Designer.UpdateQrBarcodeQuestionHandlerTests
{
    internal class when_updating_qr_barcode_question_and_title_contains_substitution_to_question_from_roster : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.Apply(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = rosterSizeQuestionId,
                GroupPublicKey = chapterId,
                IsInteger = true,
                MaxAllowedValue = 5,
                StataExportCaption = "roster_size_question"
            });
            questionnaire.Apply(new QRBarcodeQuestionAdded()
            {
                QuestionId = questionId,
                ParentGroupId = chapterId,
                Title = "old title",
                VariableName = "old_variable_name",
                IsMandatory = false,
                Instructions = "old instructions",
                EnablementCondition = "old condition",
                ResponsibleId = responsibleId
            });
            questionnaire.Apply(new NewGroupAdded { PublicKey = rosterId, ParentGroupPublicKey = chapterId});
            questionnaire.Apply(new GroupBecameARoster(responsibleId: responsibleId, groupId: rosterId));
            questionnaire.Apply(new RosterChanged(responsibleId: responsibleId, groupId: rosterId,
                rosterSizeQuestionId: rosterSizeQuestionId, rosterTitleQuestionId: null, rosterSizeSource: RosterSizeSourceType.Question,
                rosterFixedTitles: null));
            questionnaire.Apply(new NumericQuestionAdded()
            {
                PublicKey = questionFromRosterId,
                GroupPublicKey = rosterId,
                IsInteger = true,
                StataExportCaption = substitutionVariableName
            });
        };

        Because of = () =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateQRBarcodeQuestion(questionId: questionId, title: titleWithSubstitution,
                    variableName: "var",
                variableLabel: null, isMandatory: false, enablementCondition: null, instructions: null,
                    responsibleId: responsibleId, validationExpression: null, validationMessage: null));

        It should_throw_QuestionnaireException = () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        It should_throw_exception_with_message_containting__title__contains_illegal__substitution__ = () =>
             new[] { "title", "contains", "illegal", "substitution" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));

        
        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid questionFromRosterId = Guid.Parse("22222222222222222222222222222222");
        private static Guid rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string substitutionVariableName = "substitution_var";
        private static string titleWithSubstitution = string.Format("title with substitution - %{0}%", substitutionVariableName);
    }
}