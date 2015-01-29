﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelTests
{
    internal class when_setting_answer_to_nested_group_inside_roster : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");
            rosterId2 = Guid.Parse("44444444444444444444444444444444");
            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
            nestedGroupId = Guid.Parse("22222222222222222222222222222222");
            questionInNestedGroupId = Guid.Parse("11111111111111111111111111111111");
            
            var nestedGroupTitle = "nested Group";
            questionnarie = CreateQuestionnaireDocumentWithOneChapter(
                new NumericQuestion()
                {
                    PublicKey = rosterSizeQuestionId,
                    QuestionType = QuestionType.Numeric
                },
                new Group()
                {
                    PublicKey = rosterId,
                    IsRoster = true,
                    RosterSizeQuestionId = rosterSizeQuestionId,
                    Children = new List<IComposite>
                    {
                        new Group(nestedGroupTitle)
                        {
                            PublicKey = nestedGroupId,
                            Children = new List<IComposite> { new NumericQuestion() { PublicKey = questionInNestedGroupId, QuestionType = QuestionType.Numeric} }
                        }
                    }
                }, 
                new Group() { PublicKey = rosterId2, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
               answers: new AnsweredQuestionSynchronizationDto[0],
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);

            PropagateScreen(interviewViewModel, rosterId, 0);
            PropagateScreen(interviewViewModel, rosterId2, 0);
        };

        Because of = () =>
            interviewViewModel.SetAnswer(InterviewItemId.ConvertIdAndRosterVectorToString(questionInNestedGroupId, new decimal[] { 0 }), 3);

        It should_count_of_answered_questions_in_rosters_navigation_item_of_nested_group_be_equal_to_1 = () =>
            ((QuestionnaireNavigationPanelItem)((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewItemId.ConvertIdAndRosterVectorToString(rosterId, new decimal[] { 0 })]).Items[0]).Answered.ShouldEqual(1);
        
        It should_count_of_total_questions_in_rosters_navigation_item_of_nested_group_be_equal_to_1 = () =>
            ((QuestionnaireNavigationPanelItem)((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewItemId.ConvertIdAndRosterVectorToString(rosterId, new decimal[] { 0 })]).Items[0]).Total.ShouldEqual(1);

        It should_count_of_answered_questions_in_grids_first_row_of_nested_group_be_equal_to_1 = () =>
            ((QuestionnaireNavigationPanelItem)((QuestionnaireGridViewModel)interviewViewModel.Screens[InterviewItemId.ConvertIdAndRosterVectorToString(rosterId, new decimal[0])]).Rows.First().Items[0]).Answered.ShouldEqual(1);

        It should_count_of_total_questions_in_grids_first_row_of_nested_group_be_equal_to_1 = () =>
          ((QuestionnaireNavigationPanelItem)((QuestionnaireGridViewModel)interviewViewModel.Screens[InterviewItemId.ConvertIdAndRosterVectorToString(rosterId, new decimal[0])]).Rows.First().Items[0]).Total.ShouldEqual(1);

        It should_count_of_answered_questions_in_nested_group_screen_be_equal_to_1 = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewItemId.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).Answered.ShouldEqual(1);

        It should_count_of_total_questions_in_nested_group_screen_be_equal_to_1 = () =>
            ((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewItemId.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).Total.ShouldEqual(1);

        It should_answer_on_question_in_nested_group_be_equal_3_ = () =>
            ((ValueQuestionViewModel)((QuestionnairePropagatedScreenViewModel)interviewViewModel.Screens[InterviewItemId.ConvertIdAndRosterVectorToString(nestedGroupId, new decimal[] { 0 })]).Items[0]).AnswerObject.ShouldEqual(3);

        It should_answer_on_answered_question_be_equal_3_ = () =>
            interviewViewModel.FindQuestion(q => q.PublicKey == new InterviewItemId(questionInNestedGroupId, new decimal[] { 0 }))
                .First()
                .AnswerObject.ShouldEqual(3);
        
        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid rosterId2;
        private static Guid nestedGroupId;
        private static Guid questionInNestedGroupId;
    }
}
