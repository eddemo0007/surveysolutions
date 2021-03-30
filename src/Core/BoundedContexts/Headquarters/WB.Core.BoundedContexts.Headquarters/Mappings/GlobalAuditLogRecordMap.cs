﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Users.UserProfile.InterviewerAuditLog;
using WB.Infrastructure.Native.Workspaces;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class GlobalAuditLogRecordMap : ClassMapping<GlobalAuditLogRecord>
    {
        public GlobalAuditLogRecordMap()
        {
            Schema(WorkspaceConstants.SchemaName);
            
            this.Table("auditlogrecords");

            this.Id(x => x.Id, idMap =>
            {
                idMap.Generator(Generators.Identity);
                idMap.Column("Id");
            });

            this.Property(x => x.RecordId);
            this.Property(x => x.ResponsibleId);
            this.Property(x => x.ResponsibleName);
            this.Property(x => x.Time);
            this.Property(x => x.TimeUtc);
            this.Property(x => x.Type);
            this.Property("Payload", m =>
            {
                m.Column("Payload");
            });
        }
    }
}