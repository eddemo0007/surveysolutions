using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_questionnaire_has_empty_roster : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Roster(rosterId: rosterId)
            });

            verifier = CreateQuestionnaireVerifier();
        }

        private void BecauseOf() => messages = verifier.Verify(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_warning_WB0204 () =>
            messages.ShouldContainWarning("WB0204");

        [NUnit.Framework.Test] public void should_put_reference_to_empty_roster_to_message_WB0204 () =>
            messages.GetWarning("WB0204").References.Single().Id.ShouldEqual(rosterId);

        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> messages;
        static Guid rosterId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
    }
}