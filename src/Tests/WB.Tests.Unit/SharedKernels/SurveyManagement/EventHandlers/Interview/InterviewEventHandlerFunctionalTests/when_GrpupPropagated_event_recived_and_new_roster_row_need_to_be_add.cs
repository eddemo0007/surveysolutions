﻿using System;
using Machine.Specifications;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_GrpupPropagated_event_recived_and_new_roster_row_need_to_be_add : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            viewState = CreateViewWithSequenceOfInterviewData();
            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId, rosterGroupId);

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
        };

        Because of = () =>
            viewState= interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new GroupPropagated(rosterGroupId, new decimal[0], 1)));

        It should_interview_levels_count_be_equal_to_2 = () =>
            viewState.Levels.Keys.Count.ShouldEqual(2);

        It should_interview_level_with_id_0_be_present = () =>
            viewState.Levels.Keys.ShouldContain("0");

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
    }
}
