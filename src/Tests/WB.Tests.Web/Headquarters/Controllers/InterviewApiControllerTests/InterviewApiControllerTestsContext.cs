﻿using Moq;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Interviews;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Tests.Abc;
using WB.UI.Headquarters.Controllers.Api;

namespace WB.Tests.Web.Headquarters.Controllers.InterviewApiControllerTests
{
    [NUnit.Framework.TestOf(typeof(InterviewApiController))]
    internal class InterviewApiControllerTestsContext
    {
        protected static InterviewApiController CreateController(ICommandService commandService = null,
            IAuthorizedUser authorizedUser = null, ILogger logger = null,
            IAllInterviewsFactory allInterviewsViewFactory = null,
            ITeamInterviewsFactory teamInterviewViewFactory = null,
            IChangeStatusFactory changeStatusFactory = null,
            IInterviewSummaryViewFactory interviewSummaryViewFactory = null)
        {
            return new InterviewApiController(
                authorizedUser ?? Mock.Of<IAuthorizedUser>(), 
                allInterviewsViewFactory ?? Stub<IAllInterviewsFactory>.WithNotEmptyValues,
                teamInterviewViewFactory ?? Stub<ITeamInterviewsFactory>.WithNotEmptyValues,
                changeStatusFactory ?? Stub<IChangeStatusFactory>.WithNotEmptyValues,
                interviewSummaryViewFactory ?? Stub<IInterviewSummaryViewFactory>.WithNotEmptyValues);
        }
    }
}
