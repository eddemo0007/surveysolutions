using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_root_cascading_single_option_question_and_dependent_cascading_question_has_options_for_this_answer_and_it_is_disabled : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var actorId = Guid.Parse("33333333333333333333333333333333");

                Setup.MockedServiceLocator();

                var questionnaire = Create.QuestionnaireDocument(questionnaireId, new[]
                {
                    Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Option(text: "parent option 1", value: "1"),
                        Create.Option(text: "parent option 2", value: "2")
                    }),
                    Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId, options: new List<Answer>
                    {
                        Create.Option(text: "child 1 for parent option 1", value: "1.1", parentValue: "1"),
                        Create.Option(text: "child 2 for parent option 1", value: "1.2", parentValue: "1"),
                    }),
                });

                var interview = SetupInterview(questionnaire, new object[]
                {
                    Create.Event.SingleOptionQuestionAnswered(questionId: parentSingleOptionQuestionId, answer: 2, propagationVector: Empty.RosterVector),
                    Create.Event.QuestionsDisabled(Create.Identity(childCascadedComboboxId)),
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1m);

                    return new InvokeResults
                    {
                        EnabledQuestions =
                            eventContext.AnyEvent<QuestionsEnabled>()
                                ? eventContext.GetSingleEvent<QuestionsEnabled>().Questions.Select(identity => identity.Id).ToArray()
                                : null,
                    };
                }
            });

        It should_raise_QuestionsEnabled_event_for_dependent_question = () =>
            results.EnabledQuestions.ShouldContainOnly(Guid.Parse("11111111111111111111111111111111"));

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
            public Guid[] EnabledQuestions { get; set; }
        }
    }
}