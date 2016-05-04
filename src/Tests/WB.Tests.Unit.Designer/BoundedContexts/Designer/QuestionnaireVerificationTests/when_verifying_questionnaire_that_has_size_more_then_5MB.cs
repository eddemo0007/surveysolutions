﻿using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireVerificationTests
{
    internal class when_verifying_questionnaire_that_has_size_more_then_5MB : QuestionnaireVerifierTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaireDocument();
            questionnaire.Children.Add(
                new TextQuestion(new string('q', 5 * 1024 * 1024))
                {
                    StataExportCaption = "var0"
                }); 
            verifier = CreateQuestionnaireVerifier();
        };

        Because of = () => 
            verificationMessages = verifier.CheckForErrors(questionnaire);

        
        It should_return_1_message = () =>
            verificationMessages.Count().ShouldEqual(1);

        It should_return_message_with_code__WB0098 = () =>
            verificationMessages.Single().Code.ShouldEqual("WB0098");

        It should_return_WB0098_error_with_appropriate_message = () =>
            verificationMessages.Single().Message.ShouldNotBeEmpty();
        


        static QuestionnaireDocument questionnaire;
        static QuestionnaireVerifier verifier;
        static IEnumerable<QuestionnaireVerificationMessage> verificationMessages;
    }
}