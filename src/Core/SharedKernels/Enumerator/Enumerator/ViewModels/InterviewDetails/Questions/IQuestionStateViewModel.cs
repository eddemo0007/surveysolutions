﻿using System;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public interface IQuestionStateViewModel : IDisposable
    {
        QuestionHeaderViewModel Header { get; }
        ValidityViewModel Validity { get; }
        WarningsViewModel Warnings { get; }
        EnablementViewModel Enablement { get; }
        CommentsViewModel Comments { get; }
    }
}