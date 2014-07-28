﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Main.Core.Documents;
using Moq;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog;
using WB.Core.BoundedContexts.Capi.Synchronization.Implementation.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Repositories;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContext.Capi.Synchronization.Tests.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_questionnaire_in_census_mode : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            questionnaireDocument = new QuestionnaireDocument()
            {
                PublicKey = Guid.NewGuid()
            };

            var questionnaireMetadata = new QuestionnaireMetadata(1, true);

            syncItem = new SyncItem() { ItemType = SyncItemType.Template, IsCompressed = true, Content = "some content", MetaInfo = "some metadata", Id = Guid.NewGuid() };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserrialize<QuestionnaireDocument>(syncItem.Content)).Returns(questionnaireDocument);
            jsonUtilsMock.Setup(x => x.Deserrialize<QuestionnaireMetadata>(syncItem.MetaInfo)).Returns(questionnaireMetadata);

            commandService = new Mock<ICommandService>();

            plainQuestionnaireRepositoryMock = new Mock<IPlainQuestionnaireRepository>();

            changeLogManipulator = new Mock<IChangeLogManipulator>();
            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object, commandService.Object, jsonUtilsMock.Object, null,
                plainQuestionnaireRepositoryMock.Object);
        };

        Because of = () => capiDataSynchronizationService.SavePulledItem(syncItem);

        It should_call_RegisterPlainQuestionnaire_once =
            () =>
                commandService.Verify(
                    x =>
                        x.Execute(
                            Moq.It.Is<RegisterPlainQuestionnaire>(
                                param =>
                                    param.QuestionnaireId == questionnaireDocument.PublicKey && param.Version == 1 && param.AllowCensusMode == true), null),
                    Times.Once);

        It should_store_questionnaire_in_plaine_storage_once =
            () =>
                plainQuestionnaireRepositoryMock.Verify(
                    x => x.StoreQuestionnaire(questionnaireDocument.PublicKey, 1, questionnaireDocument),
                    Times.Once);

        It should_create_public_record_in_change_log_for_sync_item_once =
        () =>
            changeLogManipulator.Verify(
                x =>
                    x.CreatePublicRecord(syncItem.Id),
                Times.Once);

        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static SyncItem syncItem;
        private static QuestionnaireDocument questionnaireDocument;
        private static Mock<ICommandService> commandService;
        private static Mock<IPlainQuestionnaireRepository> plainQuestionnaireRepositoryMock;
        private static Mock<IChangeLogManipulator> changeLogManipulator;
    }
}
