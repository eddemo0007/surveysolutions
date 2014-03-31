﻿using System.Linq;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview.Base
{
    public abstract class GroupsPassiveEvent : InterviewPassiveEvent
    {
        public Identity[] Groups { get; private set; }

        protected GroupsPassiveEvent(Identity[] groups)
        {
            this.Groups = groups.ToArray();
        }
    }
}