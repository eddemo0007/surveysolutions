using Machine.Specifications;

using Moq;
using WB.Core.BoundedContexts.Tester.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;


namespace WB.Tests.Unit.BoundedContexts.Tester.ViewModels.ActiveGroupViewModelTests
{
    internal class when_initializing_active_group_view_model : ActiveGroupViewModelTestContext
    {
        Establish context = () =>
        {
            interviewRepositoryMock.Setup(x => x.Get(Moq.It.IsAny<string>())).Returns(Mock.Of<IStatefulInterview>());
            activeGroup = CreateActiveGroupViewModel(eventRegistry: eventRegistry.Object, interviewRepository: interviewRepositoryMock.Object);
        };

        Because of = () => activeGroup.Init(interviewId, navigationState.Object);

        It should_subscribe_view_model_for_events =
            () => eventRegistry.Verify(x => x.Subscribe(activeGroup, interviewId), Times.Once);

        static ActiveGroupViewModel activeGroup;

        static readonly string interviewId = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";

        static readonly Mock<NavigationState> navigationState = new Mock<NavigationState>();

        static readonly Mock<ILiteEventRegistry> eventRegistry = new Mock<ILiteEventRegistry>();
        static readonly Mock<IStatefulInterviewRepository> interviewRepositoryMock = new Mock<IStatefulInterviewRepository>();

        
    }
}