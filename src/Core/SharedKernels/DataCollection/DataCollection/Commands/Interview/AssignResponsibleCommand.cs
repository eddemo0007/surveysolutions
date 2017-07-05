﻿using System;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    public class AssignResponsibleCommand : InterviewCommand
    {
        public Guid SupervisorId { get; private set; }
        public Guid? InterviewerId { get; private set; }
        public DateTime? AssignTime { get; private set; }

        public AssignResponsibleCommand(Guid interviewId, Guid userId, Guid? interviewerId, Guid supervisorId, DateTime? assignTime)
            : base(interviewId, userId)
        {
            this.InterviewerId = interviewerId;
            this.SupervisorId = supervisorId;
            this.AssignTime = assignTime;
        }
    }
}