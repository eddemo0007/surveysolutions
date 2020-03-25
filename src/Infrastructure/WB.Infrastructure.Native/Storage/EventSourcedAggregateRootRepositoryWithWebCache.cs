using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.Caching;
using System.Threading;
using Ncqrs.Domain.Storage;
using Ncqrs.Eventing.Storage;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Infrastructure.Native.Monitoring;

namespace WB.Infrastructure.Native.Storage
{
    public class EventSourcedAggregateRootRepositoryWithWebCache : EventSourcedAggregateRootRepository,
        IAggregateRootCacheCleaner, IAggregateRootCacheFiller
    {
        private readonly IEventStore eventStore;
        private readonly IInMemoryEventStore inMemoryEventStore;
        private readonly EventBusSettings eventBusSettings;
        private readonly IServiceLocator serviceLocator;
        private readonly IAggregateLock aggregateLock;

        public EventSourcedAggregateRootRepositoryWithWebCache(IEventStore eventStore,
            IInMemoryEventStore inMemoryEventStore,
            EventBusSettings eventBusSettings,
            IDomainRepository repository,
            IServiceLocator serviceLocator,
            IAggregateLock aggregateLock)
            : base(eventStore, repository)
        {
            this.eventStore = eventStore;
            this.inMemoryEventStore = inMemoryEventStore;
            this.eventBusSettings = eventBusSettings;
            this.serviceLocator = serviceLocator;
            this.aggregateLock = aggregateLock;
        }

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId)
            => this.GetLatest(aggregateType, aggregateId, null, CancellationToken.None);

        public override IEventSourcedAggregateRoot GetLatest(Type aggregateType, Guid aggregateId,
            IProgress<EventReadingProgress> progress, CancellationToken cancellationToken)
        {
            return aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                var aggregateRoot = this.GetFromCache(aggregateId);

                if (aggregateRoot == null)
                {
                    if (!this.eventBusSettings.IsIgnoredAggregate(aggregateId))
                    {
                        aggregateRoot = base.GetLatest(aggregateType, aggregateId, progress, cancellationToken);
                    }
                    else
                    {
                        var events = this.inMemoryEventStore.Read(aggregateId, 0);
                        aggregateRoot = base.repository.Load(aggregateType, aggregateId, events);
                    }

                    if (aggregateRoot != null)
                        this.PutToCache(aggregateRoot);
                }

                return aggregateRoot;
            });
        }

        private IEventSourcedAggregateRoot GetFromCache(Guid aggregateId)
        {
            if (!(Cache.Get(Key(aggregateId)) is IEventSourcedAggregateRoot cachedAggregate))
            {
                CommonMetrics.StatefullInterviewCacheMiss.Inc();
                return null;
            }

            bool isDirty = cachedAggregate.HasUncommittedChanges() || eventStore.GetLastEventSequence(aggregateId) != cachedAggregate.Version; 

            if (isDirty)
            {
                Evict(aggregateId);
                return null;
            }

            this.serviceLocator.InjectProperties(cachedAggregate);

            CommonMetrics.StatefullInterviewCacheHit.Inc();
            return cachedAggregate;
        }

        static readonly MemoryCache Cache = MemoryCache.Default;

        protected virtual TimeSpan Expiration => TimeSpan.FromMinutes(5);

        private void PutToCache(IEventSourcedAggregateRoot aggregateRoot)
        {
            var key = Key(aggregateRoot.EventSourceId);

            Cache.Set(key, aggregateRoot, new CacheItemPolicy
            {
                RemovedCallback = OnUpdateCallback,
                SlidingExpiration = Expiration
            });

            CommonMetrics.StatefullInterviewsCached.Labels("added").Inc();
        }

        private void OnUpdateCallback(CacheEntryRemovedArguments arguments)
        {
            CacheItemRemoved(arguments.CacheItem.Key, arguments.RemovedReason);
        }

        protected virtual string Key(Guid id) => "aggregateRoot_" + id;

        protected virtual void CacheItemRemoved(string key, CacheEntryRemovedReason reason)
        {
            CommonMetrics.StatefullInterviewsCached.Labels("removed").Inc();
        }

        public void Evict(Guid aggregateId)
        {
            this.aggregateLock.RunWithLock(aggregateId.FormatGuid(), () =>
            {
                var key = Key(aggregateId);

                Cache.Remove(key);
            });
        }

        public void Store(IEventSourcedAggregateRoot aggregateRoot)
        {
            PutToCache(aggregateRoot);
        }
    }
}
