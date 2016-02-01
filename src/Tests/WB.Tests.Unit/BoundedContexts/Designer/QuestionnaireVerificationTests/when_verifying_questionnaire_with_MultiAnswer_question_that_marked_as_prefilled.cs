using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_TextList_question_that_marked_as_prefilled : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            prefilledTextListquestionId = Guid.Parse("10000000000000000000000000000000");
            textQuestionId = Guid.Parse("20000000000000000000000000000000");
            questionnaire = CreateQuestionnaireDocument();

            questionnaire.Children.Add(new TextListQuestion()
            {
                PublicKey = prefilledTextListquestionId,
                StataExportCaption = "var1",
                Featured = true
            });

            questionnaire.Children.Add(new TextListQuestion()
            {
                StataExportCaption = "var2",
                PublicKey = textQuestionId
            });

            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () =>
            resultErrors = verifier.Verify(questionnaire);

        It should_return_1_message = () =>
            resultErrors.Count().ShouldEqual(1);

        It should_return_message_with_level_general = () =>
            resultErrors.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);

        It should_return_message_with_code__WB0039 = () =>
            resultErrors.Single().Code.ShouldEqual("WB0039");

        It should_return_message_with_1_references = () =>
            resultErrors.Single().References.Count().ShouldEqual(1);

        It should_return_message_reference_with_type_Question = () =>
            resultErrors.Single().References.First().Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        It should_return_message_reference_with_id_of_prefilledTextListquestionId = () =>
            resultErrors.Single().References.First().Id.ShouldEqual(prefilledTextListquestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> resultErrors;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;

        private static Guid prefilledTextListquestionId;
        private static Guid textQuestionId;
    }
}
