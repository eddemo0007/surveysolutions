using System;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Tests.Unit.Designer.BoundedContexts.QuestionnaireTests.Attachments
{
    internal class when_cloning_questionnaire_with_attachment : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            questionnaire = CreateQuestionnaire(questionnaireId: questionnaireId, responsibleId: responsibleId);

            sourceQuestionnaire = Create.QuestionnaireDocument();
            sourceQuestionnaire.Attachments.Add(Create.Attachment(attachmentId: attachmentId, name: name, contentId: contentId));

            eventContext = new EventContext();
        };

        Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        Because of = () => 
            questionnaire.CloneQuestionnaire("Title", false, responsibleId, clonedQuestionnaireId, sourceQuestionnaire);

        It should_raise_QuestionnaireCloned_event_with_1_attachment = () =>
            eventContext.GetSingleEvent<QuestionnaireCloned>().QuestionnaireDocument.Attachments.Count.ShouldEqual(1);

        It should_set_new_AttachmentId_in_raised_event = () =>
            eventContext.GetSingleEvent<QuestionnaireCloned>().QuestionnaireDocument.Attachments.First().AttachmentId.ShouldNotEqual(attachmentId);

        It should_set_original_Name_in_raised_event = () =>
            eventContext.GetSingleEvent<QuestionnaireCloned>().QuestionnaireDocument.Attachments.First().Name.ShouldEqual(name);

        It should_set_Content_Id_in_raised_event = () =>
            eventContext.GetSingleEvent<QuestionnaireCloned>().QuestionnaireDocument.Attachments.First().ContentId.ShouldEqual(contentId);

        private static Questionnaire questionnaire;
        private static QuestionnaireDocument sourceQuestionnaire;
        private static readonly Guid responsibleId = Guid.Parse("DDDD0000000000000000000000000000");
        private static readonly Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid clonedQuestionnaireId = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid attachmentId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        private static readonly string name = "name";
        private static readonly string contentId = "content id";
        private static EventContext eventContext;
    }
}