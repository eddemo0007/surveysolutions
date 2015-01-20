﻿using System;
using System.Linq;
using Machine.Specifications;
using Ncqrs;
using Ncqrs.Eventing;
using WB.Core.Infrastructure.Storage.EventStore.Implementation;
using WB.Core.Infrastructure.Storage.Raven.Implementation.WriteSide;
using WB.Tests.Integration.StorageTests;
using WB.UI.Designer.Providers.CQRS.Accounts.Events;

namespace WB.Tests.Integration.EventStore
{
    [Subject(typeof (WriteSideEventStore))]
    [Ignore("Test is incompatible with RavenDB >:(")]
    public class when_storing_event_stream : with_in_memory_event_store
    {
        Establish context = () =>
        {
            eventSourceId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            int sequenceCounter = 1;

            NcqrsEnvironment.RegisterEventDataType(typeof(AccountRegistered));
            NcqrsEnvironment.RegisterEventDataType(typeof(AccountConfirmed));
            NcqrsEnvironment.RegisterEventDataType(typeof(AccountLocked));

            events = new UncommittedEventStream(Guid.NewGuid(), null);

            events.Append(new UncommittedEvent(Guid.NewGuid(), 
                eventSourceId, 
                sequenceCounter++, 
                0, 
                DateTime.UtcNow, 
                new AccountRegistered{ApplicationName = "App", ConfirmationToken = "token", Email = "test@test.com"}, 
                new Version(1, 0)));

            events.Append(new UncommittedEvent(Guid.NewGuid(), 
                eventSourceId, 
                sequenceCounter++, 
                0, 
                DateTime.UtcNow, 
                new AccountConfirmed(),
                new Version(1, 0)));
            events.Append(new UncommittedEvent(Guid.NewGuid(), 
                eventSourceId,
                sequenceCounter++,
                0, 
                DateTime.UtcNow,
                new AccountLocked(), 
                new Version(1, 0)));

            WriteSideEventStorage = new WriteSideEventStore(ConnectionProvider);
        };

        Because of = () => WriteSideEventStorage.Store(events);

        It should_read_stored_events = () =>
        {
            var eventStream = WriteSideEventStorage.ReadFrom(eventSourceId, 0, int.MaxValue);
            eventStream.IsEmpty.ShouldBeFalse();
            eventStream.Count().ShouldEqual(3);

            var firstEvent = eventStream.First();
            firstEvent.Payload.ShouldBeOfExactType<AccountRegistered>();
            var accountRegistered = (AccountRegistered)firstEvent.Payload;

            accountRegistered.Email.ShouldEqual("test@test.com");
        };

        Cleanup things = () => WriteSideEventStorage.Dispose();

        private static UncommittedEventStream events;
        private static WriteSideEventStore WriteSideEventStorage;
        private static Guid eventSourceId;
    }
}

