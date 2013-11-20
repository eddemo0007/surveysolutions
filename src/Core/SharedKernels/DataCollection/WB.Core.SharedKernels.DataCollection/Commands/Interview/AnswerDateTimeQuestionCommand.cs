﻿using System;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootMethod(typeof(Implementation.Aggregates.Interview), "AnswerDateTimeQuestion")]
    public class AnswerDateTimeQuestionCommand : AnswerQuestionCommand
    {
        public DateTime Answer { get; private set; }

        public AnswerDateTimeQuestionCommand(Guid interviewId, Guid userId, Guid questionId, int[] rosterVector, DateTime answerTime, DateTime answer)
            : base(interviewId, userId, questionId, rosterVector, answerTime)
        {
            this.Answer = answer;
        }
    }
}