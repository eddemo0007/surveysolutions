﻿using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Spec;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Tests.QuestionnaireTests;

namespace WB.Core.BoundedContexts.Designer.Tests.CloneGroupTests
{
    internal class when_cloning_roster_group : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            responsibleId = Guid.NewGuid();

            rosterid = Guid.NewGuid();
            questionnaire = CreateQuestionnaireWithOneRosterGroup(rosterid, responsibleId);
            questionnaire.UpdateGroup(rosterid, responsibleId, "title", rosterVariableName, null, null, null, true, RosterSizeSourceType.FixedTitles, new []{"one", "two"}, null);

            eventContext = new EventContext();
        };

        Because of = () => questionnaire.CloneGroup(Guid.NewGuid(), responsibleId, rosterid, 0);

        It should_not_clone_variable_name = () => eventContext.ShouldNotContainEvent<GroupCloned>(x => x.VariableName == rosterVariableName);

        static Questionnaire questionnaire;
        static Guid responsibleId;
        static EventContext eventContext;
        static Guid rosterid;
        static string rosterVariableName = "variable";
    }
}

