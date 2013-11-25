﻿using Ncqrs;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Implementation.ReadSide;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services;
using WB.Core.BoundedContexts.Supervisor.Implementation.Services.DataExport;
using WB.Core.BoundedContexts.Supervisor.Implementation.TemporaryDataStorage;
using WB.Core.BoundedContexts.Supervisor.Services;
using WB.Core.BoundedContexts.Supervisor.Views.DataExport;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<ISampleImportService>().To<SampleImportService>();
            this.Bind<IDataExportService>().To<DataExportService>();
            this.Bind<IEnvironmentSupplier<InterviewDataExportView>>().To<StataEnvironmentSupplier>();
            this.Bind<IExportProvider<InterviewDataExportView>>().To<IterviewExporter>();
            this.Bind(typeof(ITemporaryDataStorage<>)).To(typeof(FileTemporaryDataStorage<>));

            this.Bind(typeof(IStorageStrategy<>)).To(typeof(ReadSideStorageStrategy<>));
            this.Bind<IFunctionalDenormalizer>().To<InterviewSummaryDenormalizerFunctional>();
            this.Bind<IFunctionalDenormalizer>().To<InterviewDenormalizerFunctional>();

            this.Bind<IReadSideRepositoryReader<InterviewData>>()
                .To<InterviewDataRepositoryWriterWithCache>()
                .InSingletonScope();
        }
    }
}
