using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_TextList_question_with_maxAnswer_value_set_in_100 : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            multiAnswerQuestionWithMaxCountId = Guid.Parse("10000000000000000000000000000000");
            textQuestionId = Guid.Parse("20000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new TextListQuestion()
            {
                PublicKey = multiAnswerQuestionWithMaxCountId,
                StataExportCaption = "var1",
                MaxAnswerCount = 100
            });

            questionnaire.Children.Add(new TextListQuestion()
            {
                PublicKey = textQuestionId,
                StataExportCaption = "var2",
                MaxAnswerCount = null
            });
         
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            verificationMessages = verifier.CheckForErrors(questionnaire);

        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_level_general = () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_code__WB0042 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0042");

        It should_return_message_with_1_references = () =>
            verificationMessages.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            verificationMessages.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_multiAnswerQuestionWithMaxCountId = () =>
            verificationMessages.Single().References.First().Id.ShouldEqual(multiAnswerQuestionWithMaxCountId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid multiAnswerQuestionWithMaxCountId;
        private static Guid textQuestionId;
        
    }
}