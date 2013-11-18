﻿using System;
using System.Collections.Generic;
using Ncqrs.Commanding.CommandExecution.Mapping.Attributes;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;

namespace WB.Core.SharedKernels.DataCollection.Commands.Interview
{
    [MapsToAggregateRootConstructor(typeof(Implementation.Aggregates.Interview))]
    public class CreateInterviewForTestingCommand : InterviewCommand
    {
        public Guid Id { get; private set; }
        public Guid QuestionnaireId { get; private set; }
        
        public Dictionary<Guid, object> AnswersToFeaturedQuestions { get; private set; }
        public DateTime AnswersTime { get; private set; }

        public CreateInterviewForTestingCommand(Guid interviewId, Guid userId, Guid questionnaireId, 
            Dictionary<Guid, object> answersToFeaturedQuestions, DateTime answersTime)
            : base(interviewId, userId)
        {
            this.Id = interviewId;
            this.QuestionnaireId = questionnaireId;
            this.AnswersToFeaturedQuestions = answersToFeaturedQuestions;
            this.AnswersTime = answersTime;
            
        }
    }
}
