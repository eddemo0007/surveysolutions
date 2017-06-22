using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration;
using WB.Core.BoundedContexts.Designer.Implementation.Services.CodeGeneration.Model;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.CodeGeneratorTests
{
    internal class when_creating_models_with_multi_yesno_question : CodeGeneratorTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            AssemblyContext.SetupServiceLocator();

            questionnaire = Create.QuestionnaireDocument(questionnaireId, children: new[]
            {
                Create.Chapter(children: new IComposite[]
                {
                    Create.MultyOptionsQuestion(Id.gA, variable: "multiYesNo", yesNoView: true, validationExpression: "multiYesNo validation", enablementCondition: "multiYesNo condition", options: new List<Answer>
                    {
                        Create.Option("1", "Option 1"),
                        Create.Option("2", "Option 2")
                    }),
                })
            });

            templateModelFactory = Create.QuestionnaireExecutorTemplateModelFactory();
        }

        private void BecauseOf() =>
            model = templateModelFactory.CreateQuestionnaireExecutorTemplateModel(questionnaire, Create.CodeGenerationSettings());

        [NUnit.Framework.Test] public void should_create_model_with_1_question () =>
            model.AllQuestions.Count.ShouldEqual(1);

        [NUnit.Framework.Test] public void should_create_questionnaire_level_with_1_question () =>
            model.QuestionnaireLevelModel.Questions.Count.ShouldEqual(1);

        [NUnit.Framework.Test] public void should_reference_same_question_model_in_AllQuestions_and_questionnaire_level () =>
            model.QuestionnaireLevelModel.Questions.First().ShouldEqual(model.AllQuestions.First());

        [NUnit.Framework.Test] public void should_create_multiYesNo_question_model () =>
        {
            QuestionTemplateModel question = model.AllQuestions.Single(x => x.Id == Id.gA);
            question.VariableName.ShouldEqual("multiYesNo");
            question.ValidationExpressions.FirstOrDefault().ValidationExpression.ShouldEqual("multiYesNo validation");
            question.Condition.ShouldEqual("multiYesNo condition");
            question.IsMultiOptionYesNoQuestion.ShouldBeTrue();
            question.AllMultioptionYesNoCodes.ShouldContainOnly("1", "2");
            question.TypeName.ShouldEqual("YesNoAnswers");
            question.RosterScopeName.ShouldEqual(CodeGenerator.QuestionnaireScope);
            question.ParentScopeTypeName.ShouldEqual(CodeGenerator.QuestionnaireTypeName);
        }

        private static QuestionnaireExpressionStateModelFactory templateModelFactory;
        private static QuestionnaireExpressionStateModel model;
        private static QuestionnaireDocument questionnaire;
        private static readonly Guid questionnaireId = Guid.Parse("ffffffffffffffffffffffffffffffff");
    }
}