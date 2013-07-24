﻿using WB.Core.SharedKernel.Structures.Synchronization;

namespace WB.Core.Synchronization
{
    public interface IIncomePackagesRepository
    {
        void StoreIncomingItem(SyncItem item);
    }
}