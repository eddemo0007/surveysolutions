﻿using System;
using System.Collections.Generic;

namespace Ncqrs.Eventing.Storage
{
    public interface IStreamableEventStore : IEventStore
    {
        int CountOfAllEvents();

        IEnumerable<CommittedEvent> GetAllEvents();

        EventPosition? GetEventPosition(Guid eventStreamId, int eventSequence);
        IEnumerable<CommittedEvent> GetEventsAfterPosition(EventPosition position);
    }
}
