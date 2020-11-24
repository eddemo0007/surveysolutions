﻿#nullable enable
using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions;

namespace WB.Core.BoundedContexts.Headquarters.CalendarEvents
{
    public class CalendarEvent : IReadSideRepositoryEntity
    {

        public CalendarEvent()
        {
        }

        public CalendarEvent(Guid publicKey, DateTime start, string startTimezone, string comment, 
            Guid? interviewId, string interviewKey, int assignmentId, 
            DateTimeOffset updateDate, Guid userId)
        {
            PublicKey = publicKey;
            StartUtc = start;
            StartTimezone = startTimezone;
            Comment = comment;
            InterviewId = interviewId;
            InterviewKey = interviewKey;
            AssignmentId = assignmentId;
            CreatorUserId = userId;
            UpdateDateUtc = updateDate.UtcDateTime;
        }

        public virtual Guid PublicKey { get; set; }

        public virtual DateTime StartUtc { set; get; }
        
        public virtual string StartTimezone { set; get; } = String.Empty;

        public virtual string Comment { get; set; } = String.Empty;
        
        public virtual Guid? InterviewId { get; set; }
        public virtual string InterviewKey { get; set; } = String.Empty;
        
        public virtual int AssignmentId { get; set; }
        
        public virtual DateTime? CompletedAtUtc { get; set; }

        public virtual bool IsCompleted() => CompletedAtUtc != null;
        
        public virtual DateTime UpdateDateUtc { set; get; }
        
        public virtual Guid CreatorUserId { get; set; }
        public virtual DateTime?  DeletedAtUtc { get; set; }
        
        public virtual bool IsDeleted() => DeletedAtUtc != null;
    }
}
