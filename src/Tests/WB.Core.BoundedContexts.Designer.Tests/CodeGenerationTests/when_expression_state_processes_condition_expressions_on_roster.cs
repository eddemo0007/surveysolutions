﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationTests
{
    [Ignore("bulk test run failed on server build")]
    internal class when_expression_state_processes_condition_expressions_on_roster : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
            ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

            questionnaireDocument = CreateQuestionnairDocumenteHavingRosterWithConditions(questionnaireId, questionId, group1Id);

            IInterviewExpressionStateProvider interviewExpressionStateProvider = GetInterviewExpressionStateProvider(questionnaireDocument);


            Mock.Get(ServiceLocator.Current)
                .Setup(locator => locator.GetInstance<IInterviewExpressionStateProvider>())
                .Returns(interviewExpressionStateProvider);

            state = interviewExpressionStateProvider.GetExpressionState(questionnaireId, 0).Clone();

            state.UpdateIntAnswer(questionId, new decimal[0], 1);
            state.AddRoster(group1Id, new decimal[0], 1, null);
        };

        Because of = () =>
            state.ProcessConditionExpressions(out questionsToBeEnabled, out questionsToBeDisabled, out groupsToBeEnabled, out groupsToBeDisabled);

        It should_disabled_question_count_equal_0 = () =>
            questionsToBeDisabled.Count.ShouldEqual(0);

        It should_enabled_question_count_equal_1 = () =>
            questionsToBeEnabled.Count.ShouldEqual(1);

        It should_enabled_question_id_equal_ = () =>
            questionsToBeEnabled.Single().Id.ShouldEqual(questionId);

        It should_disabled_group_count_equal_1 = () =>
            groupsToBeDisabled.Count.ShouldEqual(1);

        It should_disabled_group_id_equal_group1id = () =>
            groupsToBeDisabled.Single().Id.ShouldEqual(group1Id);

        It should_enable_group_count_equal_0 = () =>
            groupsToBeEnabled.Count.ShouldEqual(0);


        private static Guid questionnaireId = Guid.Parse("21111111111111111111111111111111");
        private static Guid questionId = Guid.Parse("11111111111111111111111111111112");
        private static Guid group1Id = Guid.Parse("23232323232323232323232323232111");
        private static QuestionnaireDocument questionnaireDocument;

        private static IInterviewExpressionState state;
        private static List<Identity> questionsToBeEnabled;
        private static List<Identity> questionsToBeDisabled;
        private static List<Identity> groupsToBeEnabled;
        private static List<Identity> groupsToBeDisabled;
    }
}
