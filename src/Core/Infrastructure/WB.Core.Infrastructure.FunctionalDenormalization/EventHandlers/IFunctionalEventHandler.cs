using System;
using System.Collections.Generic;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.ReadSide.Repository;

namespace WB.Core.Infrastructure.FunctionalDenormalization.EventHandlers
{
    public interface IFunctionalEventHandler
    {
        void Handle(IPublishableEvent evt);
        void Handle(IEnumerable<IPublishableEvent> publishableEvents, Guid eventSourceId);
        void RegisterHandlersInOldFashionNcqrsBus(InProcessEventBus oldEventBus);
    }
    public interface IFunctionalEventHandler<T> : IFunctionalEventHandler, IEventHandler where T : class, IReadSideRepositoryEntity
    {
        void Handle(IPublishableEvent evt, IStorageStrategy<T> storage);
    }
}