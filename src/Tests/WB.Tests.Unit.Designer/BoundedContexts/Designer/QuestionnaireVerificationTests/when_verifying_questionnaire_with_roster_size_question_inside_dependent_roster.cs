using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_with_roster_size_question_inside_dependent_roster : QuestionnaireVerifierTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaireDocument(new IComposite[]
            {
                new Group
                {
                    PublicKey = rosterGroupId,
                    IsRoster = true,
                    VariableName = "a",
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        Create.NumericIntegerQuestion(
                            rosterSizeQuestionId,
                            variable: "var"
                        )
                    }.ToReadOnlyCollection()
                }
            });

            verifier = CreateQuestionnaireVerifier();
        }

        private void BecauseOf() =>
            verificationMessages = verifier.CheckForErrors(Create.QuestionnaireView(questionnaire));

        [NUnit.Framework.Test] public void should_return_1_message () =>
            verificationMessages.Count().ShouldEqual(1);

        [NUnit.Framework.Test] public void should_return_message_with_code__WB0054__ () =>
            verificationMessages.Single().Code.ShouldEqual("WB0054");

        [NUnit.Framework.Test] public void should_return_message_with_level_general () =>
            verificationMessages.Single().MessageLevel.ShouldEqual(VerificationMessageLevel.General);
        
        [NUnit.Framework.Test] public void should_return_message_with_2_references () =>
            verificationMessages.Single().References.Count().ShouldEqual(2);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_type_Roster () =>
            verificationMessages.Single().References.ElementAt(0).Type.ShouldEqual(QuestionnaireVerificationReferenceType.Roster);

        [NUnit.Framework.Test] public void should_return_first_message_reference_with_id_of_rosterId () =>
            verificationMessages.Single().References.ElementAt(0).Id.ShouldEqual(rosterGroupId);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_type_question () =>
            verificationMessages.Single().References.ElementAt(1).Type.ShouldEqual(QuestionnaireVerificationReferenceType.Question);

        [NUnit.Framework.Test] public void should_return_second_message_reference_with_id_of_rosterSizeQuestionId () =>
            verificationMessages.Single().References.ElementAt(1).Id.ShouldEqual(rosterSizeQuestionId);

        private static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
        private static QuestionnaireVerifier verifier;
        private static QuestionnaireDocument questionnaire;
        private static Guid rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static Guid rosterSizeQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}
