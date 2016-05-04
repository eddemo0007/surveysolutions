﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Domain;
using Ncqrs.Eventing;
using Ncqrs.Eventing.Storage;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using IEvent = WB.Core.Infrastructure.EventBus.IEvent;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.Infrastructure.ReadSideServiceTests
{
    internal class when_rebuild_selected_views : ReadSideServiceTestContext
    {
        Establish context = () =>
        {
            readSideRepositoryCleanerMock = new Mock<IReadSideRepositoryCleaner>();
            readSideRepositoryWriterMock = new Mock<ICacheableRepositoryWriter>();
            readSideRepositoryWriterMock.Setup(x => x.ViewType).Returns(typeof(object));

            eventHandlerMock = new Mock<IEventHandler>();

            eventHandlerMock.Setup(x => x.Writers).Returns(new object[] { readSideRepositoryCleanerMock.Object, readSideRepositoryWriterMock.Object });
            eventHandlerMock.Setup(x => x.Name).Returns(HandlerToRebuild);

            var eventHandlerToIgnoreMock = new Mock<IEventHandler>();

            eventHandlerToIgnoreMock.Setup(x => x.Writers).Returns(new object[] { readSideRepositoryCleanerMock.Object, readSideRepositoryWriterMock.Object });
            eventHandlerToIgnoreMock.Setup(x => x.Name).Returns("Other name");

            eventDispatcherMock = new Mock<IEventDispatcher>();
            eventDispatcherMock.Setup(x => x.GetAllRegistredEventHandlers()).Returns(new[] { eventHandlerMock.Object, eventHandlerToIgnoreMock.Object });

            committedEvent = new CommittedEvent(Guid.NewGuid(), "test", Guid.NewGuid(), Guid.NewGuid(), 1, DateTime.Now, 0, Mock.Of<IEvent>());
            streamableEventStoreMock = new Mock<IStreamableEventStore>();
            streamableEventStoreMock.Setup(x => x.GetAllEvents())
                .Returns(new[] { committedEvent });

            transactionManagerProviderManagerMock = Mock.Get(Mock.Of<ITransactionManagerProviderManager>(_
                => _.GetTransactionManager() == Mock.Of<ITransactionManager>()));

            readSideService = CreateReadSideService(
                eventDispatcher: eventDispatcherMock.Object,
                streamableEventStore: streamableEventStoreMock.Object,
                transactionManagerProviderManager: transactionManagerProviderManagerMock.Object);
        };

        Because of = () =>
        {
            readSideService.RebuildViewsAsync(new[] { HandlerToRebuild }, 0);
            WaitRebuildReadsideFinish(readSideService);
        };

        It should_rebuild_all_view = () =>
            readSideService.AreViewsBeingRebuiltNow().ShouldEqual(false);

        It should_call_clean_method_for_registered_writers_once = () =>
            readSideRepositoryCleanerMock.Verify(x => x.Clear(), Times.Once);

        It should_enable_cache_for_registered_writers_once = () =>
            readSideRepositoryWriterMock.Verify(x => x.EnableCache(), Times.Once);

        It should_disable_cache_for_registered_writers_once = () =>
           readSideRepositoryWriterMock.Verify(x => x.DisableCache(), Times.Once);

        It should_pin_readside_transaction_manager = () =>
            transactionManagerProviderManagerMock.Verify(
                _ => _.PinRebuildReadSideTransactionManager(), Times.Once);

        It should_unpin_transaction_manager = () =>
            transactionManagerProviderManagerMock.Verify(
                _ => _.UnpinTransactionManager(), Times.Once);

        It should_publish_one_event_on_event_dispatcher = () =>
            eventDispatcherMock.Verify(x => x.PublishEventToHandlers(committedEvent, Moq.It.Is<Dictionary<IEventHandler, Stopwatch>>(handlers => handlers.Count() == 1 && handlers.First().Key == eventHandlerMock.Object)), Times.Once);

        It should_return_readble_status = () =>
            readSideService.GetRebuildStatus().CurrentRebuildStatus.ShouldContain("Rebuild specific views succeeded.");

        private static ReadSideService readSideService;
        private static Mock<IEventDispatcher> eventDispatcherMock;
        private static Mock<IStreamableEventStore> streamableEventStoreMock;
        private static Mock<IEventHandler> eventHandlerMock;
        private static Mock<IReadSideRepositoryCleaner> readSideRepositoryCleanerMock;
        private static Mock<ICacheableRepositoryWriter> readSideRepositoryWriterMock;
        private static Mock<ITransactionManagerProviderManager> transactionManagerProviderManagerMock;

        private static CommittedEvent committedEvent;
        private static string HandlerToRebuild = "handler to rebuild";
    }
}
