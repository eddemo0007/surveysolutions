using System;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Exceptions;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireTests
{
    internal class when_deleting_macro_without_premission_to_edit : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: ownerId);
            questionnaire.AddMacro(Create.Command.AddMacro(questionnaireId, macroId, ownerId));
            questionnaire.AddSharedPerson(sharedPersonId, "email@email.com", ShareType.View, ownerId);

            deleteMacro = Create.Command.DeleteMacro(questionnaireId, macroId, sharedPersonId);
        };

        Because of = () =>
            exception = Catch.Exception(() => questionnaire.DeleteMacro(deleteMacro));

        It should_throw_exception = () =>
            exception.ShouldNotBeNull();

        It should_throw_questionnaire_exception = () =>
            exception.ShouldBeOfExactType(typeof(QuestionnaireException));

        It should_throw_exception_with_type_DoesNotHavePermissionsForEdit = () =>
            ((QuestionnaireException)exception).ErrorType.ShouldEqual(DomainExceptionType.DoesNotHavePermissionsForEdit);

        private static Exception exception;
        private static DeleteMacro deleteMacro;
        private static Questionnaire questionnaire;
        private static readonly Guid ownerId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
        private static readonly Guid sharedPersonId = Guid.Parse("EEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid macroId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
    }
}