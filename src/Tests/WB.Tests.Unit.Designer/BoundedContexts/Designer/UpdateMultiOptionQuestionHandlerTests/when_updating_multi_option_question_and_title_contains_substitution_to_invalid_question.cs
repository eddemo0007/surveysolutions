using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateMultiOptionQuestionHandlerTests
{
    internal class when_updating_multi_option_question_and_title_contains_substitution_to_invalid_question : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddQRBarcodeQuestion(questionId,
                        chapterId,
                        responsibleId,
                        title: "old title",
                        variableName: "old_variable_name",
                        instructions: "old instructions",
                        enablementCondition: "old condition");
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateMultiOptionQuestion(
                    questionId: questionId,
                    title: titleWithSubstitution,
                    variableName: variableName,
                    variableLabel: null,
                    scope: scope,
                    enablementCondition: enablementCondition,
                    hideIfDisabled: false,
                    instructions: instructions,
                    responsibleId: responsibleId
                    , options: options,
                    linkedToEntityId: linkedToQuestionId,
                    areAnswersOrdered: areAnswersOrdered,
                    maxAllowedAnswers: maxAllowedAnswers, 
                    yesNoView: yesNoView, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties()));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__title___not_existing_variable_name__as__substitution__ () =>
            new[] { "text", "contains", "unknown", "substitution" }.ShouldEachConformTo(
                keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private const string substitutionVariableName = "notExistingVar";
        private static string titleWithSubstitution = string.Format("title with substitution - %{0}%", substitutionVariableName);
        private static string variableName = "qr_barcode_question";
        private static string instructions = "intructions";
        private static QuestionScope scope = QuestionScope.Interviewer;
        private static string enablementCondition = null;
        private static Option[] options = new Option[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "2", "Option 2"), };
        private static Guid? linkedToQuestionId = (Guid?)null;
        private static bool areAnswersOrdered = false;
        private static int? maxAllowedAnswers = null;
        private static bool yesNoView = false;
    }
}