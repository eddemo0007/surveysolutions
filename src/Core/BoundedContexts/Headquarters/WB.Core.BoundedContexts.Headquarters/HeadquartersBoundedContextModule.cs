﻿using Microsoft.Practices.ServiceLocation;
using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Users.Denormalizers;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.User;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        private readonly bool supervisorFunctionsEnabled;

        public HeadquartersBoundedContextModule(bool supervisorFunctionsEnabled)
        {
            this.supervisorFunctionsEnabled = supervisorFunctionsEnabled;
        }

        public override void Load()
        {
            if (!supervisorFunctionsEnabled)
            {
                this.Bind<IVersionedQuestionnaireReader>().To<VersionedQustionnaireDocumentViewFactory>();
                this.Kernel.RegisterDenormalizer<InterviewsFeedDenormalizer>();
                this.Kernel.RegisterDenormalizer<VersionedQustionnaireDocumentDenormalizer>();
                this.Kernel.RegisterDenormalizer<UsersFeedDenormalizer>();
                this.Kernel.RegisterDenormalizer<QuestionnaireFeedDenormalizer>();
            }

            CommandRegistry.Configure<User, CreateUserCommand>(configuration => configuration.ValidatedBy<HeadquarterUserCommandValidator, CreateUserCommand>());
            CommandRegistry.Configure<User, UnarchiveUserCommand>(configuration => configuration.ValidatedBy<HeadquarterUserCommandValidator, UnarchiveUserCommand>());

            this.Bind<IUserBatchCreator>().To<UserBatchCreator>();
            this.Bind<IUserPreloadingVerifier>().To<UserPreloadingVerifier>();
            this.Bind<IUserPreloadingCleaner>().To<UserPreloadingCleaner>();
        }
    }
}
