﻿using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Tests.Abc;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.ViewModels
{
    [TestOf(typeof(WaitingForSupervisorActionViewModel))]
    internal class WaitingForSupervisorActionViewModelTests
    {
        [Test]
        public async Task when_getting_ui_items_and_view_model_has_last_visited_interview_id_then_view_model_should_have_specified_HighLightedItemIndex()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var dashboardItemsAccessor = Mock.Of<IDashboardItemsAccessor>(x =>
                x.WaitingForSupervisorAction() == new[]
                {
                    Create.ViewModel.SupervisorDashboardInterviewViewModel(Guid.NewGuid()),
                    Create.ViewModel.SupervisorDashboardInterviewViewModel(interviewId)
                });
            var viewModel = Create.ViewModel.WaitingForSupervisorActionViewModel(dashboardItemsAccessor);
            viewModel.Prepare(interviewId);
            //act
            await viewModel.Initialize();
            //assert
            Assert.That(viewModel.HighLightedItemIndex, Is.EqualTo(2));
        }
    }
}
