using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_moving_roster_size_question_to_roster_group : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            rosterGroupId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            targetRosterGroupId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");

            rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);


            AddGroup(questionnaire: questionnaire, groupId: targetRosterGroupId, parentGroupId: chapterId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: null, isRoster: true, rosterSizeSource: RosterSizeSourceType.FixedTitles,
                rosterTitleQuestionId: null, rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "fixed title 1"), new FixedRosterTitleItem("2", "test 2") });

            questionnaire.AddNumericQuestion(
                rosterSizeQuestionId,
                isInteger: true,
                parentId: targetRosterGroupId,
                responsibleId:responsibleId);
            
            AddGroup(questionnaire: questionnaire, groupId: rosterGroupId, parentGroupId: targetRosterGroupId, condition: null,
                responsibleId: responsibleId, rosterSizeQuestionId: rosterSizeQuestionId, isRoster: true);
        }


        private void BecauseOf() =>
            questionnaire.MoveQuestion(rosterSizeQuestionId, chapterId, targetIndex: 0, responsibleId: responsibleId);

        [NUnit.Framework.Test] public void should_contains_question () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterSizeQuestionId).ShouldNotBeNull();

        [NUnit.Framework.Test] public void should_contains_question_with_GroupId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterSizeQuestionId)
           .PublicKey.ShouldEqual(rosterSizeQuestionId);

        [NUnit.Framework.Test] public void should_contains_question_with_chapterId_specified () =>
            questionnaire.QuestionnaireDocument.Find<IQuestion>(rosterSizeQuestionId)
            .GetParent().PublicKey.ShouldEqual(chapterId);


        private static Questionnaire questionnaire;
        private static Guid responsibleId;
        private static Guid rosterGroupId;
        private static Guid targetRosterGroupId;
        private static Guid chapterId;
        private static Guid rosterSizeQuestionId;
    }
}