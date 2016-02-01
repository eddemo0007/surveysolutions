﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    class when_verifying_questionnaire_with_question_that_has_two_question_with_same_variable_names : QuestionnaireVerifierTestsContext
    {

        private Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocumentWithOneChapter(new Group("Chapter")
            {
                Children = new List<IComposite>()
                {
                    new NumericQuestion("first")
                    {
                        PublicKey = firstQuestionId,
                        StataExportCaption = variableName
                    },
                    new NumericQuestion("second")
                    {
                        PublicKey = secondQuestionId,
                        StataExportCaption = variableName
                    }
                }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0062 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0062");

        It should_return_message_with_level_critical = () =>
            resultErrors.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.Critical);


        It should_return_message_with_1_reference = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            resultErrors.Single()
                .References.ShouldEachConformTo(reference => reference.Type == QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_secondQuestion = () =>
            resultErrors.Single().References.ElementAt(0).Id.ShouldEqual(secondQuestionId);

        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;

        private static Guid firstQuestionId = Guid.Parse("a1111111111111111111111111111111");
        private static Guid secondQuestionId = Guid.Parse("b1111111111111111111111111111111");
        private static string variableName = "same";

    }
}
