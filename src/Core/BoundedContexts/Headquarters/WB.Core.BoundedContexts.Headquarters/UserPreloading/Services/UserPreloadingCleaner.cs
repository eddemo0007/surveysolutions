﻿using System;
using System.Linq;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Tasks;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Services
{
    internal class UserPreloadingCleaner : IUserPreloadingCleaner
    {
        private readonly IUserPreloadingService userPreloadingService;

        private readonly UserPreloadingSettings userPreloadingSettings;

        private readonly IPlainTransactionManager plainTransactionManager;

        public UserPreloadingCleaner(IUserPreloadingService userPreloadingService, IPlainTransactionManager plainTransactionManager, UserPreloadingSettings userPreloadingSettings)
        {
            this.userPreloadingService = userPreloadingService;
            this.plainTransactionManager = plainTransactionManager;
            this.userPreloadingSettings = userPreloadingSettings;
        }

        public void CleanUpInactiveUserPreloadingProcesses()
        {
            var processeIdsToClean =
                this.plainTransactionManager.ExecuteInPlainTransaction(() => userPreloadingService.GetPreloadingProcesses()
                    .Where(p => p.LastUpdateDate < DateTime.Now.AddDays(-userPreloadingSettings.HowOldInDaysProcessShouldBeInOrderToBeCleaned)).Select(p=>p.UserPreloadingProcessId)
                    .ToArray());

            foreach (var processeIdToClean in processeIdsToClean)
            {
                this.plainTransactionManager.ExecuteInPlainTransaction(
                    () => userPreloadingService.DeletePreloadingProcess(processeIdToClean));
            }
        }
    }
}