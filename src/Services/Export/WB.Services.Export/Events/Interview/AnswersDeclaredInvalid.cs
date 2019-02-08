﻿using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WB.Services.Export.Events.Interview.Base;

namespace WB.Services.Export.Events.Interview
{
    public class AnswersDeclaredInvalid : InterviewPassiveEvent
    {
        private IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> failedValidationConditions;

        public Identity[] Questions { get; set; } = new Identity[]{};

        public List<KeyValuePair<Identity, IReadOnlyList<FailedValidationCondition>>> FailedConditionsStorage { get; set; } 

        public IReadOnlyDictionary<Identity, IReadOnlyList<FailedValidationCondition>> FailedValidationConditions
        {
            get
            {
                return this.failedValidationConditions ?? 
                        (this.failedValidationConditions = this.FailedConditionsStorage.ToDictionary(x => x.Key, x => x.Value));
            }
            set
            {
                this.FailedConditionsStorage = value.ToList();
                this.failedValidationConditions = null;
            }
        }
    }
}
