﻿using Ninject.Modules;
using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.Core.GenericSubdomains.Logging.NLog
{
    public class NLogLoggingModule : NinjectModule
    {
        public NLogLoggingModule(){}

        public override void Load()
        {
            this.Bind<ILogger>().To<NLogLogger>();
        }
    }
}
