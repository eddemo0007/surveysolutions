using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateCascadingComboboxOptionsHandlerTests
{
    internal class when_updating_cascading_combobox_options_and_1_option_not_number : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddSingleOptionQuestion(
                parentQuestionId,
                chapterId,
                title: "text",
                variableName: "var",

                responsibleId: responsibleId,
                options: new Option[]
                {
                    new Option() { Title= "Option 1", Value = "1" },
                    new Option(){ Title = "Option 2", Value = "2" }
                }
            );

            questionnaire.AddSingleOptionQuestion(
                questionId,
                chapterId,
                title: "text",
                variableName: "q2",
                isFilteredCombobox: false,
                responsibleId: responsibleId,
                cascadeFromQuestionId: parentQuestionId
            );
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() =>
                questionnaire.UpdateCascadingComboboxOptions(questionId: questionId, responsibleId: responsibleId, options: options));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__value_only_number__ () =>
             new[] { "value", "only number" }.ShouldEachConformTo(
                    keyword => exception.Message.ToLower().Contains(keyword));


        private static Questionnaire questionnaire;
        private static Exception exception;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid parentQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Option[] options = new[] { new Option(Guid.NewGuid(), "1", "Option 1"), new Option(Guid.NewGuid(), "not number value", "Option 2") };
    }
}