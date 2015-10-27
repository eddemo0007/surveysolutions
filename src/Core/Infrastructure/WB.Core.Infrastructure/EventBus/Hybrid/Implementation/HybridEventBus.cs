using System;
using System.Collections.Generic;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus.Lite;

namespace WB.Core.Infrastructure.EventBus.Hybrid.Implementation
{
    public class HybridEventBus : IEventBus
    {
        private readonly ILiteEventBus liteEventBus;
        private readonly IEventBus cqrsEventBus;

        public HybridEventBus(ILiteEventBus liteEventBus, IEventBus cqrsEventBus)
        {
            this.liteEventBus = liteEventBus;
            this.cqrsEventBus = cqrsEventBus;
        }

        public CommittedEventStream CommitUncommittedEvents(IAggregateRoot aggregateRoot, string origin)
        {
            return this.liteEventBus.CommitUncommittedEvents(aggregateRoot, origin);
        }

        public void PublishCommitedEvents(CommittedEventStream committedEvents)
        {
            ActionUtils.ExecuteInIndependentTryCatchBlocks(
                () => this.liteEventBus.PublishCommitedEvents(committedEvents),
                () => this.cqrsEventBus.PublishCommitedEvents(committedEvents));
        }

        public void Publish(IPublishableEvent eventMessage)
        {
            this.cqrsEventBus.Publish(eventMessage);
        }

        public void Publish(IEnumerable<IPublishableEvent> eventMessages)
        {
            this.cqrsEventBus.Publish(eventMessages);
        }
    }
}