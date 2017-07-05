﻿using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_variable_value_for_question_in_roster : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            expectedVariableValue = 555;

            variableId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var variableRosterVector = Create.RosterVector(0);

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(new []
            {
                Create.Entity.FixedRoster(fixedTitles: new [] { Create.Entity.FixedTitle(0, "fixed")}, children: new []
                {
                    Create.Entity.Variable(variableId)
                })
            });

            interview = Setup.StatefulInterview(questionnaireDocument);
            interview.Apply(Create.Event.VariablesChanged(Create.Entity.ChangedVariable(Create.Identity(variableId, variableRosterVector), expectedVariableValue)));
        };

        Because of = () => actualVariableValue = interview.GetVariableValueByOrDeeperRosterLevel(variableId, Create.RosterVector(0, 1));

        It should_reduce_roster_vector_to_find_target_variable_value = () => actualVariableValue.ShouldEqual(expectedVariableValue);

        private static StatefulInterview interview;
        private static Guid variableId;
        private static int expectedVariableValue;
        private static object actualVariableValue;
    }
}

