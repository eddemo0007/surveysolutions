using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_question_with_validation_expression_and_without_validation_message : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionId = Guid.Parse("1111CCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            questionnaire = CreateQuestionnaireDocumentWithOneChapter(
                Create.NumericRealQuestion(
                    questionId,
                    validationExpression: validationExpression,
                    variable: "var1"
                ));

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        It should_return_messages_each_with_code__WB00107__ = () =>
            verificationMessages.ShouldContain(x =>x.Code == "WB0107");

        It should_return_messages_each_having_single_reference = () =>
            verificationMessages.Single(x => x.Code == "WB0107").References.Count().ShouldEqual(1);

        It should_return_messages_each_referencing_question = () =>
            verificationMessages.Single(x => x.Code == "WB0107").References.Single().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_referencing_first_incorrect_question = () =>
            verificationMessages.Single(x => x.Code == "WB0107").References.Single().Id.ShouldEqual(questionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid questionId;
        private const string validationExpression = "some expression";
    }
}