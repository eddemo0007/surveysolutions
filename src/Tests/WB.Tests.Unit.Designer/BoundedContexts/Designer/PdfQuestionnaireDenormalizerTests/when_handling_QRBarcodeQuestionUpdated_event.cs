﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.PdfQuestionnaireDenormalizerTests
{
    internal class when_handling_QRBarcodeQuestionUpdated_event : PdfQuestionnaireDenormalizerTestContext
    {
        Establish context = () =>
        {
            var pdfGroupView = CreateGroup(Guid.Parse(parentGroupId));
            pdfGroupView.AddChild(CreateQuestion(Guid.Parse(questionId)));
            pdfQuestionnaireDocument = CreatePdfQuestionnaire(pdfGroupView);

            var documentStorage =
                Mock.Of<IReadSideKeyValueStorage<PdfQuestionnaireView>>(
                    writer => writer.GetById(Moq.It.IsAny<string>()) == pdfQuestionnaireDocument);

            denormalizer = CreatePdfQuestionnaireDenormalizer(documentStorage: documentStorage);
        };

        Because of = () =>
            denormalizer.Handle(Create.QRBarcodeQuestionUpdatedEvent(questionId: questionId,
                questionTitle: questionTitle, questionVariable: questionVariable,
                questionConditionExpression: questionEnablementCondition));

        It should_question_not_be_null = () =>
            GetQuestion().ShouldNotBeNull();

        It should_question_type_be_QRBarcode = () =>
            GetQuestion().QuestionType.ShouldEqual(PdfQuestionType.QRBarcode);

        It should_question_title_be_equal_to_specified_title = () =>
            GetQuestion().Title.ShouldEqual(questionTitle);

        It should_question_title_be_equal_to_specified_var_name = () =>
            GetQuestion().VariableName.ShouldEqual(questionVariable);

        It should_question_title_be_equal_to_specified_enablement_condition = () =>
            GetQuestion().ConditionExpression.ShouldEqual(questionEnablementCondition);

        private static PdfQuestionView GetQuestion()
        {
            return pdfQuestionnaireDocument.GetEntityById<PdfQuestionView>(Guid.Parse(questionId));
        }

        private static PdfQuestionnaireDenormalizer denormalizer;
        private static string questionId = "11111111111111111111111111111111";
        private static string parentGroupId = "22222222222222222222222222222222";
        private static string questionTitle = "someTitle";
        private static string questionVariable = "var";
        private static string questionEnablementCondition = "some condition";
        private static PdfQuestionnaireView pdfQuestionnaireDocument;
    }
}
