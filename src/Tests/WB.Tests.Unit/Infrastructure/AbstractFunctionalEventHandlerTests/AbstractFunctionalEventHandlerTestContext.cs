﻿using Moq;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Tests.Unit.Infrastructure.AbstractFunctionalEventHandlerTests
{
    internal class AbstractFunctionalEventHandlerTestContext
    {
        protected static TestableFunctionalEventHandler CreateAbstractFunctionalEventHandler(
            IReadSideRepositoryWriter<IReadSideRepositoryEntity> readSideRepositoryWriter = null)
        {
            return new TestableFunctionalEventHandler(readSideRepositoryWriter ?? Mock.Of<IReadSideRepositoryWriter<IReadSideRepositoryEntity>>());
        }

        protected static IReadSideRepositoryEntity CreateReadSideRepositoryEntity()
        {
            return Mock.Of<IReadSideRepositoryEntity>();
        }

        protected static IPublishableEvent CreatePublishableEvent(ILiteEvent payload=null)
        {
            return Create.PublishableEvent(payload: payload);
        }
    }
}
