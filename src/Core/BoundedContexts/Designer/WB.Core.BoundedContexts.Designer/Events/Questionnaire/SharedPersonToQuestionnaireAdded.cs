﻿namespace Main.Core.Events.Questionnaire
{
    using System;

    public class SharedPersonToQuestionnaireAdded
    {
        public Guid PersonId { get; set; }
        public string Email { get; set; }
    }
}