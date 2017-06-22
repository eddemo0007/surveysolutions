using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_section_with_too_long_title_in_translation : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(
                translations: new[] { Create.Translation(name: "normal name") },
                children: new IComposite[] { Create.Section(title: "normal title"), });
            var translatedQuestionnaire = Create.QuestionnaireDocumentWithOneChapter(
                translations: new[] { Create.Translation(name: "normal name") },
                children: new IComposite[] { Create.Section(title: "long title" + new string('l', 500)), });

            var questionnaireTranslator = Setup.QuestionnaireTranslator(questionnaire, null, translatedQuestionnaire);
            verifier = CreateQuestionnaireVerifier(questionnaireTranslator: questionnaireTranslator);
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();

        [NUnit.Framework.Test] public void should_return_messages_with_codes__WB0260 () =>
            verificationMessages.Select(message => message.Code).ShouldContain("WB0260");

        [NUnit.Framework.Test] public void should_return_messages_with_translation_name () =>
            verificationMessages.Single(message => message.Code == "WB0260").Message.ShouldContain("normal name");


        private static QuestionnaireDocument questionnaire;
        private static QuestionnaireVerifier verifier;
        private static List<QuestionnaireVerificationMessage> verificationMessages;
    }
}