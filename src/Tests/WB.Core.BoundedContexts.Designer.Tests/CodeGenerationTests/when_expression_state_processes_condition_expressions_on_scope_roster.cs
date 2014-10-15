﻿using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using WB.Core.SharedKernels.DataCollection;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Designer.Tests.CodeGenerationTests
{
    internal class when_expression_state_processes_condition_expressions_on_scope_roster : CodeGenerationTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = RemoteFunc.Invoke(appDomainContext.Domain, () =>
            {
                Guid questionnaireId = Guid.Parse("21111111111111111111111111111111");
                Guid question1Id = Guid.Parse("11111111111111111111111111111112");
                Guid group1Id = Guid.Parse("23232323232323232323232323232111");
                Guid question2Id = Guid.Parse("11111111111111111111111111111113");
                Guid group2Id = Guid.Parse("63232323232323232323232323232111");

                var serviceLocatorMock = new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock };
                ServiceLocator.SetLocatorProvider(() => serviceLocatorMock.Object);

                QuestionnaireDocument questionnaireDocument = CreateQuestionnairDocumenteHavingTwoRostersInOneScopeWithConditions(questionnaireId, question1Id, group1Id, question2Id, group2Id);
                IInterviewExpressionState state = GetInterviewExpressionState(questionnaireDocument);

                state.UpdateNumericIntegerAnswer(question1Id, new decimal[0], 1);
                state.AddRoster(group1Id, new decimal[0], 1, null);
                state.AddRoster(group2Id, new decimal[0], 1, null);

                state.UpdateNumericIntegerAnswer(question2Id, new decimal[] { 1 }, 1);

                EnablementChanges enablementChanges = state.ProcessEnablementConditions();

                return new InvokeResults
                {
                    QuestionsToBeDisabledCount = enablementChanges.QuestionsToBeDisabled.Count,
                    QuestionsToBeEnabledCount = enablementChanges.QuestionsToBeEnabled.Count,
                    GroupsToBeDisabledCount = enablementChanges.GroupsToBeDisabled.Count,
                    DisabledGroupId = enablementChanges.GroupsToBeDisabled.Single().Id,
                    GroupsToBeEnabledCount = enablementChanges.GroupsToBeEnabled.Count,
                    EnabledGroupId = enablementChanges.GroupsToBeEnabled.Single().Id,
                };
            });

        It should_disabled_question_count_equal_0 = () =>
            results.QuestionsToBeDisabledCount.ShouldEqual(0);

        It should_enabled_question_count_equal_2 = () =>
            results.QuestionsToBeEnabledCount.ShouldEqual(2);

        It should_disabled_group_count_equal_1 = () =>
            results.GroupsToBeDisabledCount.ShouldEqual(1);

        It should_disabled_group_id_equal_group2id = () =>
            results.DisabledGroupId.ShouldEqual(Guid.Parse("63232323232323232323232323232111"));

        It should_enable_group_count_equal_1 = () =>
            results.GroupsToBeEnabledCount.ShouldEqual(1);

        It should_enabled_group_id_equal_group1id = () =>
            results.EnabledGroupId.ShouldEqual(Guid.Parse("23232323232323232323232323232111"));


        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static AppDomainContext appDomainContext;
        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public int QuestionsToBeDisabledCount { get; set; }
            public int QuestionsToBeEnabledCount { get; set; }
            public int GroupsToBeDisabledCount { get; set; }
            public Guid DisabledGroupId { get; set; }
            public int GroupsToBeEnabledCount { get; set; }
            public Guid EnabledGroupId { get; set; }
        }
    }
}
