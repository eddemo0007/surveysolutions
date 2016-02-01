using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_that_referenced_by_question_with_substitutions_does_not_exist : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionWithNotExistingSubstitutionsId = Guid.Parse("10000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new SingleQuestion()
            {
                PublicKey = questionWithNotExistingSubstitutionsId,
                StataExportCaption = "var",
                QuestionText = "hello %unknown%!",
                Answers = { new Answer() { AnswerValue = "1", AnswerText = "opt 1" }, new Answer() { AnswerValue = "2", AnswerText = "opt 2" } }
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0017 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0017");

        It should_return_message_with_level_general = () =>
            resultErrors.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_questionWithNotExistingSubstitutionsId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(questionWithNotExistingSubstitutionsId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid questionWithNotExistingSubstitutionsId;
    }
}
