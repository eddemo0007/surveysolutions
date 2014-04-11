﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Moq;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Repositories;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.InterviewTests
{
    internal class when_answering_qr_barcode_question_and_answer_is_specified : InterviewTestsContext
    {
        Establish context = () =>
        {
            var questionnaireId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDD0000000000");
            var questionnaire = Mock.Of<IQuestionnaire>
                (_
                    => _.HasQuestion(questionId) == true &&
                        _.GetQuestionType(questionId) == QuestionType.QRBarcode
                );

            SetupInstanceToMockedServiceLocator<IQuestionnaireRepository>(
                CreateQuestionnaireRepositoryStubWithOneQuestionnaire(questionnaireId, questionnaire));

            interview = CreateInterview(questionnaireId: questionnaireId);

            eventContext = new EventContext();
        };

        Because of = () =>
            interview.AnswerQRBarcodeQuestion(userId: userId, questionId: questionId, answerTime: answerTime,
                                              rosterVector: propagationVector, answer: answer);

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        It should_raise_QRBarcodeQuestionAnswered_event = () =>
            eventContext.ShouldContainEvent<QRBarcodeQuestionAnswered>();

        It should_raise_QRBarcodeQuestionAnswered_event_with_QuestionId_equal_to_questionId = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().QuestionId.ShouldEqual(questionId);

        It should_raise_QRBarcodeQuestionAnswered_event_with_UserId_equal_to_userId = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().UserId.ShouldEqual(userId);

        It should_raise_QRBarcodeQuestionAnswered_event_with_PropagationVector_equal_to_propagationVector = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().PropagationVector.ShouldEqual(propagationVector);

        It should_raise_QRBarcodeQuestionAnswered_event_with_AnswerTime_equal_to_answerTime = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().AnswerTime.ShouldEqual(answerTime);

        It should_raise_QRBarcodeQuestionAnswered_event_with_Answer_equal_to_answer = () =>
            eventContext.GetSingleEvent<QRBarcodeQuestionAnswered>().Answer.ShouldEqual(answer);

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId = Guid.Parse("FFFFFFFFFFFFFFFFFFFFFF1111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static decimal[] propagationVector = new decimal[0];
        private static DateTime answerTime = DateTime.Now;
        private static string answer = "some answer here";
    }
}