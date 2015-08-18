﻿using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;

namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class GroupsEnabled : GroupsPassiveEvent
    {
        public GroupsEnabled(Identity[] groups)
            : base(groups) {}
    }
}