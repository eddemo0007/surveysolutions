﻿using Machine.Specifications;
using Main.Core.View;
using Moq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Web.Api;
using WB.UI.Headquarters.API;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api;
using It = Machine.Specifications.It;

namespace WB.UI.Headquarters.Tests.ApiTests
{
    internal class when_intervews_controller_interviews_filtered_with_empty_params : ApiTestContext
    {
        private Establish context = () =>
        {
            allInterviewsViewFactory = new Mock<IViewFactory<AllInterviewsInputModel, AllInterviewsView>>();
            controller = CreateInterviewsController(allInterviewsViewViewFactory : allInterviewsViewFactory.Object);
        };

        Because of = () =>
        {
            actionResult = controller.InterviewsFiltered();
        };

        It should_return_InterviewApiView = () =>
            actionResult.ShouldBeOfExactType<InterviewApiView>();

        It should_call_factory_load_once = () =>
            allInterviewsViewFactory.Verify(x => x.Load(Moq.It.IsAny<AllInterviewsInputModel>()), Times.Once());

        private static InterviewApiView actionResult;
        private static InterviewsController controller;

        private static Mock<IViewFactory<AllInterviewsInputModel, AllInterviewsView>> allInterviewsViewFactory;
    }
}
