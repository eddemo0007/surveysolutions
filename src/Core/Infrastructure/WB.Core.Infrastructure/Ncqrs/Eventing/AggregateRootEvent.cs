using System;
using Ncqrs.Eventing;

namespace Main.Core.Events
{
    public class AggregateRootEvent
    {
        public AggregateRootEvent()
        {
        }

        public AggregateRootEvent(CommittedEvent committedEvent)
        {
            this.Payload = committedEvent.Payload;
            this.EventIdentifier = committedEvent.EventIdentifier;
            this.EventSequence = committedEvent.EventSequence;
            this.EventSourceId = committedEvent.EventSourceId;
            this.EventTimeStamp = committedEvent.EventTimeStamp;
            this.EventVersion = committedEvent.EventVersion;
            this.CommitId = committedEvent.CommitId;
            this.Origin = committedEvent.Origin;
        }

        public Guid CommitId { get; set; }

        public string Origin { get; set; }

        public Guid EventIdentifier { get; set; }

        public long EventSequence { get; set; }

        public Guid EventSourceId { get; set; }

        public DateTime EventTimeStamp { get; set; }

        public Version EventVersion { get; set; }

        public object Payload { get; set; }
        
        public UncommittedEvent CreateUncommitedEvent(long eventSequence, long initialVersionOfEventSource,
                                                      DateTime? eventTimestamp = null)
        {
            return new UncommittedEvent(
                this.EventIdentifier,
                this.EventSourceId,
                eventSequence,
                initialVersionOfEventSource,
                eventTimestamp ?? this.EventTimeStamp,
                this.Payload,
                this.EventVersion);
        }
    }
}