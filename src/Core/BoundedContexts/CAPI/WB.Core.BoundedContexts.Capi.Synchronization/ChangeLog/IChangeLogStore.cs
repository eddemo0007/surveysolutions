using System;
using Main.Core.Events;
using WB.Core.Infrastructure.Backup;

namespace WB.Core.BoundedContexts.Capi.Synchronization.ChangeLog
{
    public interface IChangeLogStore:IBackupable
    {
        void SaveChangeset(AggregateRootEvent[] recordData, Guid recordId);
        string GetChangesetContent(Guid recordId);
        void DeleteDraftChangeSet(Guid recordId);
    }
}