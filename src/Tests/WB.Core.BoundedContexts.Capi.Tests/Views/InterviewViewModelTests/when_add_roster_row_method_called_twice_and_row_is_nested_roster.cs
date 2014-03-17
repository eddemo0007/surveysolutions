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

namespace WB.Core.BoundedContexts.Capi.Tests.Views.InterviewViewModelTests
{
    internal class when_add_roster_row_method_called_twice_and_row_is_nested_roster : InterviewViewModelTestContext
    {
        Establish context = () =>
        {
            rosterId = Guid.Parse("10000000000000000000000000000000");

            var rosterSizeQuestionId = Guid.Parse("33333333333333333333333333333333");
            var nestedRosterSizeQuestionId = Guid.Parse("43333333333333333333333333333333");
            nestedRosterId = Guid.Parse("22222222222222222222222222222222");

            nestedGroupTitle = "nested Group";
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
                            PublicKey = nestedRosterId,
                            IsRoster = true,
                            RosterSizeQuestionId = nestedRosterSizeQuestionId
                        },
                        new NumericQuestion()
                        {
                            PublicKey = nestedRosterSizeQuestionId,
                            QuestionType = QuestionType.Numeric
                        }
                    }
                });

            rosterStructure = CreateQuestionnaireRosterStructure(questionnarie);

            interviewSynchronizationDto = CreateInterviewSynchronizationDto(
               answers: new AnsweredQuestionSynchronizationDto[0],
               propagatedGroupInstanceCounts: new Dictionary<InterviewItemId, RosterSynchronizationDto[]>());

            interviewViewModel = CreateInterviewViewModel(questionnarie, rosterStructure,
             interviewSynchronizationDto);

            PropagateScreen(interviewViewModel, rosterId, 0);
            PropagateScreen(interviewViewModel, rosterId, 1);
            PropagateScreen(interviewViewModel, nestedRosterId, 0, new decimal[] { 0 });
        };

        Because of = () =>
            PropagateScreen(interviewViewModel, nestedRosterId, 0, new decimal[] { 1 });

        It should_row_count_at_first_nested_roster_be_equal_to_1 = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[new InterviewItemId(nestedRosterId, new decimal[] { 0 })]).Rows.Count().ShouldEqual(1);

        It should_row_count_at_second_nested_roster_be_equal_to_1 = () =>
            ((QuestionnaireGridViewModel)interviewViewModel.Screens[new InterviewItemId(nestedRosterId, new decimal[] { 1 })]).Rows.Count().ShouldEqual(1);


        private static InterviewViewModel interviewViewModel;
        private static QuestionnaireDocument questionnarie;
        private static QuestionnaireRosterStructure rosterStructure;
        private static InterviewSynchronizationDto interviewSynchronizationDto;

        private static Guid rosterId;
        private static Guid nestedRosterId;
        private static string nestedGroupTitle;
    }
}
