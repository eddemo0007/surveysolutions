﻿using System;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Capi.ChangeLog;
using WB.Core.BoundedContexts.Capi.Implementation.Services;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.SurveySolutions.Services;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Capi.CapiDataSynchronizationServiceTests
{
    internal class when_sync_package_contains_information_about_questionnaire_assembly : CapiDataSynchronizationServiceTestContext
    {
        Establish context = () =>
        {
            var meta = new QuestionnaireAssemblyMetadata(questionnaireId, version);

            syncItem = new SyncItem
            {
                RootId = questionnaireId.Combine(version),
                ItemType = SyncItemType.QuestionnaireAssembly,
                IsCompressed = false,
                Content = "some_content",
                MetaInfo = "dummy meta"
            };

            var jsonUtilsMock = new Mock<IJsonUtils>();
            jsonUtilsMock.Setup(x => x.Deserialize<QuestionnaireAssemblyMetadata>(Moq.It.IsAny<string>())).Returns(meta);
            
            changeLogManipulator = new Mock<IChangeLogManipulator>();

            questionnareAssemblyFileAccessor = new Mock<IQuestionnaireAssemblyFileAccessor>();

            capiDataSynchronizationService = CreateCapiDataSynchronizationService(changeLogManipulator.Object,
                jsonUtils : jsonUtilsMock.Object, questionnareAssemblyFileAccessor: questionnareAssemblyFileAccessor.Object);
        };

        Because of = () => capiDataSynchronizationService.SavePulledItem(syncItem);

        It should_call_StoreAssembly_once =
            () =>
                questionnareAssemblyFileAccessor.Verify(
                    x => x.StoreAssembly(questionnaireId, version, assemblyAsBase64),
                    Times.Once);
        
        private static Guid questionnaireId = Guid.Parse("11111111111111111111111111111111");
        private static long version = 3;
        private static CapiDataSynchronizationService capiDataSynchronizationService;
        private static SyncItem syncItem;
        private static Mock<IChangeLogManipulator> changeLogManipulator;

        private static string assemblyAsBase64 = "some_content";
        private static Mock<IQuestionnaireAssemblyFileAccessor> questionnareAssemblyFileAccessor;
    }
}
