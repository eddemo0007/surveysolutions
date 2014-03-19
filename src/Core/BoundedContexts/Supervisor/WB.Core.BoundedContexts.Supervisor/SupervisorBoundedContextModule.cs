﻿using System;
using Ncqrs;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Factories;
using WB.Core.BoundedContexts.Supervisor.Implementation.Factories;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.TabletInformation;
using WB.Core.BoundedContexts.Supervisor.Implementation.TemporaryDataStorage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.FunctionalDenormalization.Implementation.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Synchronization;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : NinjectModule
    {
        private readonly string currentFolderPath;

        public SupervisorBoundedContextModule(string currentFolderPath)
        {
            this.currentFolderPath = currentFolderPath;
        }

        public override void Load()
        {
            this.Bind<ISampleImportService>().To<SampleImportService>();
            this.Bind<IDataExportService>().To<FileBasedDataExportService>().WithConstructorArgument("folderPath", currentFolderPath);

            this.Bind(typeof (ITemporaryDataStorage<>)).To(typeof (FileTemporaryDataStorage<>));

            Action<Guid, long> additionalEventChecker = this.AdditionalEventChecker;


            this.Bind<IQuestionnaireCacheInitializer>().To<QuestionnaireCacheInitializer>();
            this.Bind<IReadSideRepositoryReader<InterviewData>>()
                .To<ReadSideRepositoryReaderWithSequence<InterviewData>>().InSingletonScope()
                .WithConstructorArgument("additionalEventChecker", additionalEventChecker);

            this.Bind<ITabletInformationService>().To<FileBasedTabletInformationService>().WithConstructorArgument("parentFolder", currentFolderPath);
            this.Bind<IDataFileExportService>().To<CsvDataFileExportService>();
            this.Bind<IEnvironmentContentService>().To<StataEnvironmentContentService>();
            this.Bind<IExportViewFactory>().To<ExportViewFactory>();
            this.Bind<IReferenceInfoForLinkedQuestionsFactory>().To<ReferenceInfoForLinkedQuestionsFactory>();

            this.Bind<IHeadquartersSynchronizer>().To<HeadquartersSynchronizer>();

            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable
        }

        protected void AdditionalEventChecker(Guid interviewId, long sequence)
        {
            Kernel.Get<IIncomePackagesRepository>().ProcessItem(interviewId, sequence);
        }
    }
}
