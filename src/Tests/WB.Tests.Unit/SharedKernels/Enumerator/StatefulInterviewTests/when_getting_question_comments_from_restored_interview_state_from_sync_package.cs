using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{
    internal class when_getting_commented_by_supervisor_questions_from_restored_from_sync_package_interview : StatefulInterviewTestsContext
    {
        Establish context = () =>
        {
            IQuestionnaireStorage questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, _
                =>  _.IsInterviewierQuestion(superQuestionId) == false
                   && _.IsInterviewierQuestion(prefilledQuestionId) == false
                   && _.IsInterviewierQuestion(hiddenQuestionId) == false
                   && _.IsInterviewierQuestion(disabledQuestionId) == true
                   && _.IsInterviewierQuestion(interQuestionId) == true
                   && _.IsInterviewierQuestion(repliedByInterQuestionId) == true
                   && _.GetParentGroup(repliedByInterQuestionId) == parentGroupId
                   && _.GetParentGroup(interQuestionId) == parentGroupId
                   && _.GetParentGroup(interFromMissingGroupQuestionId) == missingParentGroupId
                   && _.HasGroup(parentGroupId) == true
                   && _.HasGroup(missingParentGroupId) == false
                   );

            interview = Create.AggregateRoot.StatefulInterview(
                questionnaireId: questionnaireId,
                questionnaireRepository: questionnaireRepository);

            var answersDtos = new[]
            {
                Create.Entity.AnsweredQuestionSynchronizationDto(superQuestionId, rosterVector, 1, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor)),
                Create.Entity.AnsweredQuestionSynchronizationDto(prefilledQuestionId, rosterVector, 2, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor)),
                Create.Entity.AnsweredQuestionSynchronizationDto(hiddenQuestionId, rosterVector, 3, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor)),
                Create.Entity.AnsweredQuestionSynchronizationDto(disabledQuestionId, rosterVector, 4, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor)),
                Create.Entity.AnsweredQuestionSynchronizationDto(repliedByInterQuestionId, rosterVector, 5, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor), Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Operator)),
                Create.Entity.AnsweredQuestionSynchronizationDto(interQuestionId, rosterVector, 6, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor)),
                Create.Entity.AnsweredQuestionSynchronizationDto(interFromMissingGroupQuestionId, rosterVector, 7, Create.Entity.CommentSynchronizationDto(userRole: UserRoles.Supervisor))
            };

            interview.RestoreInterviewStateFromSyncPackage(userId, Create.Entity.InterviewSynchronizationDto(
                questionnaireId: questionnaireId,
                userId: userId,
                answers: answersDtos,
                disabledQuestions: new HashSet<InterviewItemId> { Create.Entity.InterviewItemId(disabledQuestionId, rosterVector) }));
        };

        Because of = () =>
            commentedQuestionsIdentities = interview.GetCommentedBySupervisorQuestionsInInterview().ToArray();

        It should_return_2_commented_by_supervisor_questions = () =>
            commentedQuestionsIdentities.Length.ShouldEqual(2);

        It should_return_only_interviewers_questions = () =>
            commentedQuestionsIdentities.Select(x => x.Id).ShouldContainOnly(repliedByInterQuestionId, interQuestionId);

        It should_return_question_without_interviewer_interviewer_reply_first = () =>
            commentedQuestionsIdentities.ElementAt(0).Id.ShouldEqual(interQuestionId);

        private static StatefulInterview interview;
        private static Identity[] commentedQuestionsIdentities;
        private static readonly Guid superQuestionId = Guid.Parse("00000000000000000000000000000001");
        private static readonly Guid prefilledQuestionId = Guid.Parse("00000000000000000000000000000002");
        private static readonly Guid hiddenQuestionId = Guid.Parse("00000000000000000000000000000003");
        private static readonly Guid disabledQuestionId = Guid.Parse("00000000000000000000000000000006");
        private static readonly Guid interQuestionId = Guid.Parse("00000000000000000000000000000007");
        private static readonly Guid repliedByInterQuestionId = Guid.Parse("00000000000000000000000000000008");
        private static readonly Guid interFromMissingGroupQuestionId = Guid.Parse("00000000000000000000000000000009");
        private static readonly Guid parentGroupId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid missingParentGroupId = Guid.Parse("22222222222222222222222222222222");
        private static readonly decimal[] rosterVector = new decimal[] { 1m, 0m };
        private static readonly Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid userId = Guid.Parse("99999999999999999999999999999999");
    }
}