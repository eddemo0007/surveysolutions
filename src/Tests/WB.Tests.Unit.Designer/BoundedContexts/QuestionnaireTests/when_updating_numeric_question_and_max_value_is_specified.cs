﻿using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_numeric_question_and_max_value_is_specified : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(new NewGroupAdded { PublicKey = chapterId });
            questionnaire.AddQuestion(Create.Event.NewQuestionAdded(publicKey: questionId, groupPublicKey: chapterId, questionType: QuestionType.Text));
        };

        Because of = () =>
            questionnaire.UpdateNumericQuestion(questionId, "title",
                "var1",null, false, QuestionScope.Interviewer, null, false, null, new QuestionProperties(false, false),
                responsibleId, true, null, validationConditions: new List<ValidationCondition>()
                );


        It should_contains_question = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId).ShouldNotBeNull();

        It should_contains_question_with_PublicKey_equal_to_question_id = () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(questionId)
                .PublicKey.ShouldEqual(questionId);

        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid responsibleId;
    }
}