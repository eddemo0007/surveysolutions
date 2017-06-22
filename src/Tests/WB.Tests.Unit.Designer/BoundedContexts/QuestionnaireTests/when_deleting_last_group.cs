using System;
using Machine.Specifications;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests
{
    internal class when_deleting_last_remaining_group_in_questionnaire : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsible = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            rootSectionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            questionnaire = Create.Questionnaire(
                responsible,
                Create.QuestionnaireDocument(Guid.NewGuid(), Create.Group(groupId: rootSectionId, title: "last section")));
        }

        private void BecauseOf() => exception = Catch.Only<QuestionnaireException>(() => questionnaire.DeleteGroup(rootSectionId, responsible));

        [NUnit.Framework.Test] public void should_not_allow_delete_last_remaining_root_section () =>
        {
            exception.ShouldNotBeNull();
            exception.Message.ShouldEqual("Last existing section can not be removed from questionnaire");
        }

        static Questionnaire questionnaire;
        static Guid responsible;
        static Guid rootSectionId;
        static QuestionnaireException exception;
    }
}