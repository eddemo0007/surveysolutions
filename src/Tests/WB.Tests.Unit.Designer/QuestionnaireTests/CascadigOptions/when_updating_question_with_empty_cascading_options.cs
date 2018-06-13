using System;
using System.Linq;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.CascadigOptions
{
    internal class when_updating_question_with_empty_cascading_options : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            rootGroupId = Guid.Parse("00000000000000000000000000000000");
            actorId = Guid.Parse("11111111111111111111111111111111");
            questionnaire = CreateQuestionnaireWithOneGroup(actorId, groupId: rootGroupId);

            parentQuestionId = Guid.Parse("22222222222222222222222222222222");
            updatedQuestionId = Guid.Parse("33333333333333333333333333333333");

            questionnaire.AddSingleOptionQuestion(
                parentQuestionId,
                rootGroupId,
                actorId,
                options : new Option[] {
                    new Option()
                    {
                        Title = "one",
                        Value = "1"
                    },
                    new Option()
                    {
                        Title = "two",
                        Value = "2"
                    }
                }
            );

            questionnaire.AddSingleOptionQuestion(updatedQuestionId,
                rootGroupId,
                actorId,
                options: new Option[] {
                    new Option{Title = "one",Value = "1"},
                    new Option{Title = "two",Value = "2"}
                });
            BecauseOf();
        }

        private void BecauseOf() => questionnaire.UpdateSingleOptionQuestion(
            updatedQuestionId,
            "title",
            "var",
            null,
            false,
            QuestionScope.Interviewer,
            null,
            false,
            null,
            actorId,
            new[]
            {
                new Option(String.Empty, String.Empty, (decimal?)null), 
                new Option(String.Empty, String.Empty, (decimal?)null), 
                new Option(String.Empty, String.Empty, (decimal?)null) 
            }, 
            null,
            false,
            cascadeFromQuestionId: parentQuestionId, validationConditions: new System.Collections.Generic.List<WB.Core.SharedKernels.QuestionnaireEntities.ValidationCondition>(),
                linkedFilterExpression: null, properties: Create.QuestionProperties());


        [NUnit.Framework.Test] public void should_contains_question_with_empty_answers () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(updatedQuestionId).Answers.Count().Should().Be(2);


        private static Questionnaire questionnaire;
        private static Guid parentQuestionId;
        private static Guid rootGroupId;
        private static Guid updatedQuestionId;
        private static Guid actorId;
    }
}

