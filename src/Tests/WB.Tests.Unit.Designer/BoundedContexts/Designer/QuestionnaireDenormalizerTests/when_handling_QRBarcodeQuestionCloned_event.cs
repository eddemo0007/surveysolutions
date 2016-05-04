﻿using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;
using WB.Core.BoundedContexts.Designer.Implementation.Factories;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Document;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;
using it = Moq.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class when_handling_QRBarcodeQuestionCloned_event : QuestionnaireDenormalizerTestsContext
    {
        Establish context = () =>
        {
            questionFactory = new Mock<IQuestionnaireEntityFactory>();

            questionFactory.Setup(x => x.CreateQuestion(it.IsAny<QuestionData>()))
                .Callback((QuestionData qd) => questionData = qd)
                .Returns(CreateQRBarcodeQuestion(
                    questionId: questionId,
                    enablementCondition: condition,
                    instructions: instructions,
                    title: title,
                    variableName: variableName));

            @event = ToPublishedEvent(new QRBarcodeQuestionCloned()
            {
                QuestionId = questionId,
                EnablementCondition = condition,
                Instructions = instructions,
                Title = title,
                VariableName = variableName,
                ParentGroupId = parentGroupId,
                SourceQuestionId = sourceQuestionId,
                TargetIndex = targetIndex,
                ValidationExpression = validation,
                ValidationMessage = validationMessage
            });

            var questionnaireDocument = CreateQuestionnaireDocument(new[]
            {
                CreateGroup(groupId: parentGroupId,
                    children: new IComposite[]
                    {
                        new NumericQuestion(), 
                        new QRBarcodeQuestion()
                        { 
                            PublicKey = sourceQuestionId, 
                            StataExportCaption = "old_var_name",
                            QuestionText = "old title",
                            ConditionExpression = "old condition",
                            Instructions = "old instructions"
                        }
                    })
            });

            var documentStorage = new Mock<IReadSideKeyValueStorage<QuestionnaireDocument>>();
            
            documentStorage
                .Setup(writer => writer.GetById(Moq.It.IsAny<string>()))
                .Returns(questionnaireDocument);
            
            documentStorage
                .Setup(writer => writer.Store(Moq.It.IsAny<QuestionnaireDocument>(), Moq.It.IsAny<string>()))
                .Callback((QuestionnaireDocument document, string id) =>
                {
                    questionnaireView = document;
                });

            denormalizer = CreateQuestionnaireDenormalizer(documentStorage: documentStorage.Object, questionnaireEntityFactory: questionFactory.Object);
        };

        Because of = () =>
            denormalizer.Handle(@event);

        It should_pass_PublicKey_equals_questionId_to_question_factory = () =>
            questionData.PublicKey.ShouldEqual(questionId);

        It should_pass_QuestionType_equals_QRBarcode_to_question_factory = () =>
           questionData.QuestionType.ShouldEqual(QuestionType.QRBarcode);

        It should_pass_QuestionText_equals_questionId_to_question_factory = () =>
           questionData.QuestionText.ShouldEqual(title);

        It should_pass_StataExportCaption_equals_questionId_to_question_factory = () =>
           questionData.StataExportCaption.ShouldEqual(variableName);

        It should_pass_ConditionExpression_equals_questionId_to_question_factory = () =>
           questionData.ConditionExpression.ShouldEqual(condition);

        It should_pass_Instructions_equals_questionId_to_question_factory = () =>
           questionData.Instructions.ShouldEqual(instructions);

        It should_call_question_factory_ones = () =>
            questionFactory.Verify(x => x.CreateQuestion(it.IsAny<QuestionData>()), Times.Once);

        It should__not_be_null_qr_barcode_question_from_questionnaire__ = ()=>
            GetQRBarcodeQuestionById().ShouldNotBeNull();

        It should_set_questionId_as_default_value_for__PublicKey__field = () =>
           GetQRBarcodeQuestionById().PublicKey.ShouldEqual(questionId);

        It should_parent_group_exists_in_questionnaire = () =>
           questionnaireView.Find<IGroup>(parentGroupId).ShouldNotBeNull();

        It should_parent_group_contains_qr_barcode_question = () =>
           questionnaireView.Find<IGroup>(parentGroupId).Find<IQRBarcodeQuestion>(questionId);

        It should_set_target_index_to_2 = () =>
           questionnaireView.Find<IGroup>(parentGroupId).Children.IndexOf(GetQRBarcodeQuestionById()).ShouldEqual(targetIndex);

        It should_set_validation_value_for__ValidationExpression__field = () => 
           questionData.ValidationConditions.First().Expression.ShouldEqual(validation);

        It should_set_validationMessage_value_for__ValidationMessage__field = () =>
            questionData.ValidationConditions.First().Message.ShouldEqual(validationMessage);

        It should_set_Interviewer_as_default_value_for__QuestionScope__field = () =>
            GetQRBarcodeQuestionById().QuestionScope.ShouldEqual(QuestionScope.Interviewer);

        It should_set_false_as_default_value_for__Featured__field = () =>
            GetQRBarcodeQuestionById().Featured.ShouldBeFalse();

        It should_set_QRBarcode_as_default_value_for__QuestionType__field = () =>
            GetQRBarcodeQuestionById().QuestionType.ShouldEqual(QuestionType.QRBarcode);

        It should_set_varibleName_as_value_for__StataExportCaption__field = () =>
            GetQRBarcodeQuestionById().StataExportCaption.ShouldEqual(variableName);

        It should_set_title_as_value_for__QuestionText__field = () =>
            GetQRBarcodeQuestionById().QuestionText.ShouldEqual(title);

        It should_set_instructions_as_value_for__Instructions__field = () =>
            GetQRBarcodeQuestionById().Instructions.ShouldEqual(instructions);

        It should_set_condition_value_for__ConditionExpression__field = () =>
            GetQRBarcodeQuestionById().ConditionExpression.ShouldEqual(condition);

        private static IQRBarcodeQuestion GetQRBarcodeQuestionById()
        {
            return questionnaireView.FirstOrDefault<IQRBarcodeQuestion>(question => question.PublicKey == questionId);
        }

        private static QuestionData questionData;
        private static Mock<IQuestionnaireEntityFactory> questionFactory;
        private static QuestionnaireDocument questionnaireView;
        private static QuestionnaireDenormalizer denormalizer;
        private static IPublishedEvent<QRBarcodeQuestionCloned> @event;
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static Guid sourceQuestionId = Guid.Parse("22222222222222222222222222222222");
        private static Guid parentGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static string variableName = "qr_barcode_question";
        private static string title = "title";
        private static string instructions = "intructions";
        private static string condition = "condition";

        private static string validation = "validation";
        private static string validationMessage = "validationMessage";
        private static int targetIndex = 2;
    }
}