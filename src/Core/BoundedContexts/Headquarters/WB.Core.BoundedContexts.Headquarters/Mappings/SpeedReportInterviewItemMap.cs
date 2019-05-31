﻿using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories;

namespace WB.Core.BoundedContexts.Headquarters.Mappings
{
    public class SpeedReportInterviewItemMap : ClassMapping<SpeedReportInterviewItem>
    {
        public SpeedReportInterviewItemMap()
        {
            Id(x => x.InterviewId);
            Property(x => x.QuestionnaireId, pm => pm.Column(cm => cm.Index("idx_speedreportinterviewitems_questionnaireid")));
            Property(x => x.QuestionnaireVersion, pm => pm.Column(cm => cm.Index("idx_speedreportinterviewitems_questionnaireversion")));
            Property(x => x.FirstAnswerDate, pm => pm.Column(cm => cm.Index("idx_speedreportinterviewitems_firstanswerdate")));
            Property(x => x.CreatedDate);
            Property(x => x.InterviewerId);
            Property(x => x.InterviewerName);
            Property(x => x.SupervisorId);
            Property(x => x.SupervisorName);
            
            OneToOne(x=> x.InterviewSummary, m =>
            {
                m.PropertyReference(r => r.SummaryId);
                m.Access(Accessor.ReadOnly);
                m.Cascade(Cascade.None);
            });
        }
    }
}