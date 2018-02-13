﻿using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Base;


namespace WB.Core.SharedKernels.DataCollection.Events.Interview
{
    public class StaticTextsDeclaredImplausible : InterviewPassiveEvent
    {
        private IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditionsDictionary;

        public List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> FailedValidationConditions { get; protected set; }

        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> GetFailedValidationConditionsDictionary()
            => this.failedValidationConditionsDictionary ?? (this.failedValidationConditionsDictionary = this.FailedValidationConditions.ToDictionary());

        protected StaticTextsDeclaredImplausible()
        {
            this.FailedValidationConditions = new List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>>(); ;
        }

        public StaticTextsDeclaredImplausible(List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> failedValidationConditions)
        {
            this.FailedValidationConditions = failedValidationConditions;
        }
    }
}