﻿using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_answering_integer_question_A_and_that_answer_disables_question_B_and_disabled_B_disables_question_C_and_B_was_answered : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var userId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var answerTime = new DateTime(2014, 1, 1);

                var questionAId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var questionBId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var questionCId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Unit.Create.Entity.NumericIntegerQuestion(questionAId, "a"),
                    Unit.Create.Entity.NumericIntegerQuestion(questionBId, "b", enablementCondition: "a > 0"),
                    Unit.Create.Entity.NumericIntegerQuestion(questionCId, "c", enablementCondition: "b > 0")
                );

                var interview = SetupInterview(questionnaireDocument, new List<object>
                {
                    Unit.Create.Event.QuestionsEnabled(new []
                    {
                        Create.Identity(questionAId),
                        Create.Identity(questionBId),
                        Create.Identity(questionCId)
                    }),
                    Unit.Create.Event.NumericIntegerQuestionAnswered(
                        questionBId, null, 0, null, null
                    )
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, questionAId, Empty.RosterVector, answerTime, 0);

                    return new InvokeResults
                    {
                        QuestionBDisabled = GetFirstEventByType<QuestionsDisabled>(eventContext.Events).Questions.First(q => q.Id == questionBId) != null,
                        QuestionCDisabled = GetFirstEventByType<QuestionsDisabled>(eventContext.Events).Questions.First(q => q.Id == questionCId) != null,
                    };
                }
            });

        It should_disable_question_B = () =>
            results.QuestionBDisabled.ShouldBeTrue();

        It should_disable_question_C = () =>
            results.QuestionCDisabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionBDisabled { get; set; }
            public bool QuestionCDisabled { get; set; }
        }
    }
}