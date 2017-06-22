using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Exceptions;

using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.UpdateNumericQuestionHandlerTests
{
    internal class when_updating_numeric_question_with_title_which_contains_roster_title_as_substitution_reference : QuestionnaireTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            responsibleId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
            chapterId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            questionId = Guid.Parse("11111111111111111111111111111111");

            questionnaire = CreateQuestionnaire(responsibleId: responsibleId);
            questionnaire.AddGroup(chapterId, responsibleId:responsibleId);

            questionnaire.AddDefaultTypeQuestionAdnMoveIfNeeded(Create.Command.AddDefaultTypeQuestion(questionnaire.Id, questionId, "title", responsibleId, chapterId));
            
            eventContext = new EventContext();
        }

        private void BecauseOf() => exception = 
            Catch.Exception(() =>
            questionnaire.UpdateNumericQuestion(
                    new UpdateNumericQuestion(questionnaire.Id,
                    questionId,
                    responsibleId,
                    new CommonQuestionParameters()
                    {
                        Title = questionTitle,
                        VariableName = "var"
                    },
                    false, QuestionScope.Interviewer, false, false, null,
                    validationConditions: new List<ValidationCondition>())));

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        }

        [NUnit.Framework.Test] public void should_throw_QuestionnaireException () =>
            exception.ShouldBeOfExactType<QuestionnaireException>();

        [NUnit.Framework.Test] public void should_throw_exception_with_message_containting__unknown__and__substitution__ () =>
            new[] { "unknown", "substitution" }.ShouldEachConformTo(
           keyword => exception.Message.ToLower().Contains(keyword));

        private static EventContext eventContext;
        private static Questionnaire questionnaire;
        private static Guid questionId;
        private static Guid chapterId;
        private static Guid responsibleId;
        private static string questionTitle = "title %rostertitle%";
        private static Exception exception;
    }
}
