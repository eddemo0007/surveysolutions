﻿using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.DesignerEngineVersionServiceTests
{
    [TestFixture]
    internal class DesignerEngineVersionServiceNUnitTests
    {
        private DesignerEngineVersionService CreateDesignerEngineVersionService(
            IAttachmentService attachments = null)
        {
            return new DesignerEngineVersionService(attachments ?? Mock.Of<IAttachmentService>());
        }

        [Test]
        public void should_return_version_24_when_non_image_attachment_exists()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter();
            var contentId = "contentId";
            questionnaire.Attachments.Add(Create.Attachment(Id.gA, contentId: contentId));

            var attachmentContent = Create.AttachmentContent(contentType: "video/mp4", contentId: contentId);

            var attachmentService = Mock.Of<IAttachmentService>(x => x.GetContent(contentId) == attachmentContent);

            var service = this.CreateDesignerEngineVersionService(attachmentService);
 
            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
 
            Assert.That(contentVersion, Is.EqualTo(24));
        }

        [Test]
        public void should_return_version_25_when_section_has_variable_name()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.Group(variable:"test")
            });
            

            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(25));
        }

        [Test]
        public void should_return_version_26_when_contains_multioption_as_combobox()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.MultyOptionsQuestion(filteredCombobox:true)
                });


            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(26));
        }

        [Test]
        public void should_return_version_26_when_contains_singleoption_show_as_list()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.SingleOptionQuestion(showAsList:true)
                });


            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(26));
        }

        [Test]
        public void should_return_27_when_self_is_used_in_substitutions()
        {
            QuestionnaireDocument questionnaire = Create.QuestionnaireDocumentWithOneChapter(children:
                new IComposite[]{
                    Create.SingleOptionQuestion(title: "%self%")
                });


            var service = this.CreateDesignerEngineVersionService();

            // act 
            var contentVersion = service.GetQuestionnaireContentVersion(questionnaire);
            //aaa
            Assert.That(contentVersion, Is.EqualTo(27));
        }

    }
}
