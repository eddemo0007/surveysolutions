﻿using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group
{
    [Serializable]
    [MapsToAggregateRootMethod(typeof(Aggregates.Questionnaire), "MoveGroup")]
    public class MoveGroupCommand : GroupCommand
    {
        public MoveGroupCommand(Guid questionnaireId, Guid groupId, Guid? targetGroupId, int targetIndex, Guid responsibleId)
            : base(questionnaireId, groupId, responsibleId)
        {
            this.TargetGroupId = targetGroupId;
            this.TargetIndex = targetIndex;
        }

        public Guid? TargetGroupId { get; private set; }
        public int TargetIndex { get; private set; }
    }
}