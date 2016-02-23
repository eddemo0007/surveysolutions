﻿extern alias datacollection;
using System;
using Main.Core.Documents;
using Moq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Commands;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Aggregates;
using QuestionnaireDeleted = WB.Core.SharedKernels.DataCollection.Events.Questionnaire.QuestionnaireDeleted;
using TemplateImported = datacollection::Main.Core.Events.Questionnaire.TemplateImported;

namespace WB.Tests.Unit.SharedKernels.DataCollection.QuestionnaireTests
{
    internal class ImportQuestionnaireTests : QuestionnaireTestsContext
    {
        [SetUp]
        public void SetUp()
        {
            AssemblyContext.SetupServiceLocator();
        }

        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_Imported_with_specified_version_Then_QuestionnaireDeleted_Event_is_Published_with_specified_version()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);
            QuestionnaireDocument newState = CreateQuestionnaireDocumentWithOneChapter();

            using (var eventContext = new EventContext())
            {
                questionnaire.ImportFromDesigner(Create.Event.ImportFromDesigner(responsibleId, newState, false, "base64 string of assembly", 1));
                questionnaire.DisableQuestionnaire(new DisableQuestionnaire(Guid.NewGuid(), 1, responsibleId));
                // act
                questionnaire.DeleteQuestionnaire(new DeleteQuestionnaire(Guid.NewGuid(), 1, responsibleId));
                // assert
                var lastEvent = GetLastEvent<QuestionnaireDeleted>(eventContext);
            
                Assert.That(lastEvent.QuestionnaireVersion, Is.EqualTo(1));
            }
        }

        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_Imported_with_specified_responsible_Then_QuestionnaireDeleted_Event_is_Published_with_specified_responsible()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);
            var newState = CreateQuestionnaireDocumentWithOneChapter();

            using (var eventContext = new EventContext())
            {
                questionnaire.ImportFromDesigner(Create.Event.ImportFromDesigner(responsibleId, newState, false, "base64 string of assembly", 1));
                questionnaire.DisableQuestionnaire(new DisableQuestionnaire(Guid.NewGuid(), 1, responsibleId));
                // act
                questionnaire.DeleteQuestionnaire(new DeleteQuestionnaire(Guid.NewGuid(), 1, responsibleId));
                // assert
                var lastEvent = GetLastEvent<QuestionnaireDeleted>(eventContext);

                Assert.That(lastEvent.ResponsibleId, Is.EqualTo(responsibleId));
            }
        }

        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_Imported_but_not_disabled_for_delete_with_specified_responsible_Then_QuestionnaireException_thrown()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);
            var newState = CreateQuestionnaireDocumentWithOneChapter();

            using (var eventContext = new EventContext())
            {
                questionnaire.ImportFromDesigner(Create.Event.ImportFromDesigner(responsibleId, newState, false, "base64 string of assembly", 1));

                TestDelegate act = () => questionnaire.DeleteQuestionnaire(new DeleteQuestionnaire(Guid.NewGuid(), 1, responsibleId));

                // assert
                Assert.Throws<QuestionnaireException>(act);
            }
        }

        [Test]
        public void DisableQuestionnaire_When_Questionnaire_is_absent_Then_QuestionnaireException_thrown()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);

            using (var eventContext = new EventContext())
            {

                TestDelegate act = () => questionnaire.DisableQuestionnaire(new DisableQuestionnaire(Guid.NewGuid(), 3, responsibleId));

                // assert
                Assert.Throws<QuestionnaireException>(act);
            }
        }

        [Test]
        public void DisableQuestionnaire_When_Valid_Questionnaire_Imported_but_already_disabled_Then_QuestionnaireException_thrown()
        {
            // arrange
            var responsibleId = Guid.Parse("11111111111111111111111111111111");
            Questionnaire questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);
            var newState = CreateQuestionnaireDocumentWithOneChapter();

            using (var eventContext = new EventContext())
            {
                questionnaire.ImportFromDesigner(Create.Event.ImportFromDesigner(responsibleId, newState, false, "base64 string of assembly", 1));
                questionnaire.DisableQuestionnaire(new DisableQuestionnaire(Guid.NewGuid(), 1,
                    responsibleId));
                TestDelegate act =
                    () =>
                        questionnaire.DisableQuestionnaire(new DisableQuestionnaire(Guid.NewGuid(), 1, responsibleId));

                // assert
                Assert.Throws<QuestionnaireException>(act);
            }
        }

        [Test]
        public void DeleteQuestionnaire_When_Valid_Questionnaire_Version_is_invalid_Imported_Then_QuestionnaireException_sould_be_thrown()
        {
            // arrange
            Questionnaire questionnaire = CreateImportedQuestionnaire();

            // act

            Assert.Throws<QuestionnaireException>(() => questionnaire.DeleteQuestionnaire(new DeleteQuestionnaire(Guid.NewGuid(), 2, null)));
        }

        [Test]
        public void RegisterPlainQuestionnaire_When_Valid_Questionnaire_Imported_Then_Correct_Event_is_Published()
        {

            using (var eventContext = new EventContext())
            {
                // arrange
                var document = CreateQuestionnaireDocumentWithOneChapter();
                var plainQuestionnaireRepository = Mock.Of<IPlainQuestionnaireRepository>(_
                    => _.GetQuestionnaireDocument(document.PublicKey, 3) == Mock.Of<QuestionnaireDocument>());
                Questionnaire questionnaire = CreateImportedQuestionnaire(plainQuestionnaireRepository: plainQuestionnaireRepository);

                // act
                questionnaire.RegisterPlainQuestionnaire(new RegisterPlainQuestionnaire(document.PublicKey, 3, false, "dummy assembly"));

                // assert
                Assert.That(GetLastEvent<PlainQuestionnaireRegistered>(eventContext).AllowCensusMode, Is.EqualTo(false));
                Assert.That(GetLastEvent<PlainQuestionnaireRegistered>(eventContext).Version, Is.EqualTo(3));
            }
        }


        [Test]
        public void
            RegisterPlainQuestionnaire_When_Valid_Questionnaire_Is_Deleted_From_Plain_Storage_Imported_Then_Correct_Event_is_Published()
        {
            // arrange
            var document = CreateQuestionnaireDocumentWithOneChapter();
            document.IsDeleted = true;

            var plainQuestionnaireRepository =
                Mock.Of<IPlainQuestionnaireRepository>(_ => _.GetQuestionnaireDocument(document.PublicKey, 3) == document);

            Setup.InstanceToMockedServiceLocator<IPlainQuestionnaireRepository>(plainQuestionnaireRepository);


            Questionnaire questionnaire = CreateImportedQuestionnaire();

            // act and assert
            Assert.Throws<QuestionnaireException>(() => questionnaire.RegisterPlainQuestionnaire(new RegisterPlainQuestionnaire(document.PublicKey, 3, false, null)));
        }

        [Test]
        public void ImportFromDesigner_When_Valid_Questionnaire_but_previouse_version_was_deleted_Imported_Then_Correct_Event_is_Published()
        {

            using (var eventContext = new EventContext())
            {
                // arrange
                var responsibleId = Guid.Parse("11111111111111111111111111111111");
                Questionnaire questionnaire = CreateImportedQuestionnaire(creatorId: responsibleId);
                var document = CreateQuestionnaireDocumentWithOneChapter();

                // act
                questionnaire.ImportFromDesigner(Create.Event.ImportFromDesigner(responsibleId, document, false, "base64 string of assembly", 1));
                questionnaire.DisableQuestionnaire(new DisableQuestionnaire(Guid.NewGuid(), 2, responsibleId));
                questionnaire.ImportFromDesigner(Create.Event.ImportFromDesigner(responsibleId, document, false, "base64 string of assembly", 1));

                // assert
                Assert.That(GetLastEvent<TemplateImported>(eventContext).Version, Is.EqualTo(3));
            }
        }

    }
}