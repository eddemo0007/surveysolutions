﻿using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.Interview.InterviewEventHandlerFunctionalTests
{
    internal class when_SingleOptionQuestionAnswered_recived_and_answered_question_is_roster_title : InterviewEventHandlerFunctionalTestContext
    {
        Establish context = () =>
        {
            rosterGroupId = Guid.Parse("10000000000000000000000000000000");
            rosterScopeId = Guid.Parse("12222222222222222222222222222222");
            rosterTitleQuestionId = Guid.Parse("13333333333333333333333333333333");
            viewState = CreateViewWithSequenceOfInterviewData();
            var questionnaireRosterStructure = CreateQuestionnaireRosterStructure(rosterScopeId,
                new Dictionary<Guid, RosterTitleQuestionDescription>
                {
                    {
                        rosterGroupId,
                        new RosterTitleQuestionDescription(rosterTitleQuestionId,
                            new Dictionary<decimal, string> { { 1, "one" }, { 2, "two" }, { 3, "three" } })
                    }
                });

            interviewEventHandlerFunctional = CreateInterviewEventHandlerFunctional(questionnaireRosterStructure);
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(Create.RosterInstancesAdded(rosterGroupId)));
        };

        Because of = () =>
            viewState = interviewEventHandlerFunctional.Update(viewState,
                CreatePublishableEvent(new SingleOptionQuestionAnswered(Guid.NewGuid(), rosterTitleQuestionId, new decimal[] { 0 }, DateTime.Now,
                    selectedOption)));

        It should_roster_title_be_equal_to_recived_answer_text_representation = () =>
            viewState.Levels["0"].RosterRowTitles[rosterGroupId].ShouldEqual(selectedOptionText);

        It should_answer_on_head_question_be_equal_to_recived_answer = () =>
            viewState.Levels["0"].GetAllQuestions().First(q => q.Id == rosterTitleQuestionId).Answer.ShouldEqual(selectedOption);

        private static InterviewEventHandlerFunctional interviewEventHandlerFunctional;
        private static InterviewData viewState;
        private static Guid rosterGroupId;
        private static Guid rosterScopeId;
        private static Guid rosterTitleQuestionId;
        private static decimal selectedOption = 2;
        private static string selectedOptionText = "two";
    }
}
