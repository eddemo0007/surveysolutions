﻿using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class DashboardItemViewModel
    {
        public string QuestionariName { get; set; }

        public string InterviewId { get; set; }

        public InterviewStatus Status { get; set; }

        public DateTime? StartedDate { get; set; }
        public DateTime? ComplitedDate { get; set; }
        public DateTime? RejectedDate { get; set; }
    }
}