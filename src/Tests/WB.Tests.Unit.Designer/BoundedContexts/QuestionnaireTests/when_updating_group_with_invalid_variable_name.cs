using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;


namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_updating_group_with_invalid_variable_name : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);
            questionnaire.AddGroup(rosterId, chapterId, variableName: "valid", responsibleId: responsibleId, isRoster:true);
        }

        private void BecauseOf() =>
            exception = Catch.Exception(() => questionnaire.UpdateGroup(
                groupId: rosterId,
                responsibleId: responsibleId,
                title: "title",
                variableName: "this",
                rosterSizeQuestionId: null,
                description: null,
                condition: null,
                hideIfDisabled: false,
                isRoster: true,
                rosterSizeSource: RosterSizeSourceType.FixedTitles,
                rosterFixedTitles: new[] { new FixedRosterTitleItem("1", "1"), new FixedRosterTitleItem("2", "2") },
                rosterTitleQuestionId: null));

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting_message_about_csharp_keywords () =>
            exception.Message.ToLower().ShouldContain("variable name or roster id shouldn't match with keywords");

        private static Exception exception;
        private static Questionnaire questionnaire;
        private static Guid responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
        private static Guid chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static Guid rosterId = Guid.Parse("11111111111111111111111111111111");
        private static Guid rosterSizeQuestionId = Guid.Parse("22222222222222222222222222222222");
    }
}