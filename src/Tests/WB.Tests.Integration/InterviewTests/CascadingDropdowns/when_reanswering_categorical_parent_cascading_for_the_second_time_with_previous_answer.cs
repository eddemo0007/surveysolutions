using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    [Subject(typeof(Interview))]
    internal class when_reanswering_categorical_parent_cascading_for_the_second_time_with_previous_answer : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                Setup.MockedServiceLocator();
                var parentSingleOptionQuestionId = Guid.Parse("11111111111111111111111111111111");
                var childCascadedComboboxId = Guid.Parse("22222222222222222222222222222222");
                var grandChildCascadedComboboxId = Guid.Parse("33333333333333333333333333333333");

                var questionnaireId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                var questionnaire = Create.QuestionnaireDocument(questionnaireId,
                    Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Option(text: "parent option 1", value: "1"),
                        Create.Option(text: "parent option 2", value: "2")
                    }),
                    Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                        options: new List<Answer>
                                 {
                                     Create.Option(text: "child 1 for parent option 1", value: "1.1", parentValue: "1"),
                                     Create.Option(text: "child 2 for parent option 1", value: "1.2", parentValue: "1"),
                                     Create.Option(text: "child 1 for parent option 2", value: "2.1", parentValue: "2"),
                                     Create.Option(text: "child 2 for parent option 2", value: "2.1", parentValue: "2"),
                                 }),
                    Create.SingleQuestion(grandChildCascadedComboboxId, "q3", cascadeFromQuestionId: childCascadedComboboxId,
                        options: new List<Answer>
                                 {
                                     Create.Option(text: "grand child 1 for parent option 1.1", value: "11.1", parentValue: "1.1"),
                                     Create.Option(text: "grand child 2 for parent option 1.1", value: "11.2", parentValue: "1.1"),
                                     Create.Option(text: "grand child 3 for parent option 1.2", value: "12.1", parentValue: "1.2"),
                                     Create.Option(text: "grand child 4 for parent option 1.2", value: "12.2", parentValue: "1.2"),
                                     Create.Option(text: "grand child 1 for parent option 2.1", value: "21.1", parentValue: "2.1"),
                                     Create.Option(text: "grand child 2 for parent option 2.1", value: "21.2", parentValue: "2.1"),
                                     Create.Option(text: "grand child 3 for parent option 2.2", value: "22.1", parentValue: "2.2"),
                                     Create.Option(text: "grand child 4 for parent option 2.2", value: "22.2", parentValue: "2.2"),
                                 })
                    );

                var interview = SetupInterview(questionnaire, new List<object>());

                interview.AnswerSingleOptionQuestion(userId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);
                interview.AnswerSingleOptionQuestion(userId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 1.2m);
                interview.AnswerSingleOptionQuestion(userId, grandChildCascadedComboboxId, new decimal[] { }, DateTime.Now, 12.2m);
                interview.AnswerSingleOptionQuestion(userId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 2);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);

                    return new InvokeResults
                    {
                        WasChildCascadingEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasChildCascadingDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasGrandChildAnswerDiasbled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                        WasGrandChildAnswerEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId))
                    };
                }
            });


        It should_not_enable_child_question_because_it_was_already_enabled = () =>
            results.WasChildCascadingEnabled.ShouldBeFalse();

        It should_not_disable_child_question_because_it_was_already_enabled = () =>
            results.WasChildCascadingDisabled.ShouldBeFalse();

        It should_not_disable_grandchild_question_because_it_was_already_enabled = () =>
            results.WasGrandChildAnswerDiasbled.ShouldBeFalse();

        It should_not_enable_grandchild_question_because_it_was_already_enabled = () =>
            results.WasGrandChildAnswerEnabled.ShouldBeFalse();

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
            public bool WasChildCascadingEnabled { get; set; }
            public bool WasChildCascadingDisabled { get; set; }
            public bool WasGrandChildAnswerEnabled { get; set; }
            public bool WasGrandChildAnswerDiasbled { get; set; }
        }
    }
}