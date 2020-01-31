﻿using System.Collections.Generic;
using System.Net.Http;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.DataExport.Security;
using WB.Core.BoundedContexts.Headquarters.Factories;
using WB.Core.BoundedContexts.Headquarters.Implementation;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.Preloading;
using WB.Core.BoundedContexts.Headquarters.Users;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Views;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc.Storage;
using WB.UI.Headquarters.Code.CommandTransformation;
using WB.UI.Headquarters.Controllers;
using WB.UI.Headquarters.Controllers.Api.DataCollection.Interviewer;
using WB.UI.Headquarters.Services;
using AssignmentsController = WB.UI.Headquarters.Controllers.Api.PublicApi.AssignmentsController;

namespace WB.Tests.Web.TestFactories
{
    internal class ControllerFactory
    {
        public ReportsController ReportsController(
            IMapReport mapReport = null,
            IAllUsersAndQuestionnairesFactory allUsersAndQuestionnairesFactory = null,
            IAuthorizedUser authorizedUser = null,
            IUserViewFactory userViewFactory = null,
            ITeamUsersAndQuestionnairesFactory teamUsersAndQuestionnairesFactory = null)
        {
            return new ReportsController(
                mapReport ?? Mock.Of<IMapReport>(),
                Mock.Of<IChartStatisticsViewFactory>(),
                allUsersAndQuestionnairesFactory ?? Mock.Of<IAllUsersAndQuestionnairesFactory>(_ => _.Load() == new AllUsersAndQuestionnairesView() { Questionnaires = new TemplateViewItem[0] }),
                new TestInMemoryWriter<InterviewSummary>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(), 
                authorizedUser ?? Mock.Of<IAuthorizedUser>());
        }

        public InterviewerControllerBase InterviewerApiController(ITabletInformationService tabletInformationService = null,
            IUserViewFactory userViewFactory = null,
            IInterviewerSyncProtocolVersionProvider syncVersionProvider = null,
            IAuthorizedUser authorizedUser = null,
            SignInManager<HqUser> signInManager = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            IAssignmentsService assignmentsService = null,
            IInterviewInformationFactory interviewInformationFactory = null,
            IPlainKeyValueStorage<InterviewerSettings> interviewerSettings = null,
            IPlainKeyValueStorage<TenantSettings> tenantSettings = null,
            IInterviewerVersionReader interviewerVersionReader = null,
            IUserToDeviceService userToDeviceService = null)
        {
            var result = new InterviewerControllerBase(
                tabletInformationService ?? Mock.Of<ITabletInformationService>(),
                userViewFactory ?? Mock.Of<IUserViewFactory>(),
                syncVersionProvider ?? new InterviewerSyncProtocolVersionProvider(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                questionnaireBrowseViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(x =>
                    x.GetByIds(It.IsAny<QuestionnaireIdentity[]>()) == new List<QuestionnaireBrowseItem>()),
                interviewInformationFactory ?? Mock.Of<IInterviewInformationFactory>(),
                assignmentsService ?? Mock.Of<IAssignmentsService>(),
                Mock.Of<IClientApkProvider>(),
                interviewerSettings ?? Mock.Of<IPlainKeyValueStorage<InterviewerSettings>>(),
                tenantSettings ?? new InMemoryKeyValueStorage<TenantSettings>(),
                interviewerVersionReader ?? Mock.Of<IInterviewerVersionReader>(),
                userToDeviceService ?? Mock.Of<IUserToDeviceService>()
            );

            result.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            return result;
        }

        public AssignmentsController AssignmentsPublicApiController(
            IAssignmentViewFactory assignmentViewFactory = null,
            IAssignmentsService assignmentsService = null,
            IMapper mapper = null,
            IUserRepository userManager = null,
            IQuestionnaireStorage questionnaireStorage = null,
            ISystemLog auditLog = null,
            IInterviewCreatorFromAssignment interviewCreatorFromAssignment = null,
            IPreloadedDataVerifier verifier = null,
            ICommandTransformator commandTransformator = null,
            ICommandService commandService = null,
            IAuthorizedUser authorizedUser = null
            )
        {
            var result = new AssignmentsController(assignmentViewFactory,
                assignmentsService,
                mapper,
                userManager,
                questionnaireStorage,
                auditLog,
                interviewCreatorFromAssignment,
                verifier,
                Abc.Create.Service.AssignmentFactory(),
                Mock.Of<IInvitationService>(),
                Mock.Of<IAssignmentPasswordGenerator>(),
                commandService ?? Mock.Of<ICommandService>(),
                authorizedUser ?? Mock.Of<IAuthorizedUser>(),
                commandTransformator
                );

            return result;
        }
    }
}
