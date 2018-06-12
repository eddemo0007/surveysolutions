﻿using Android.Graphics;
using Android.Support.V4.Content;
using Android.Widget;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class TextViewAnswerStateTextColorBinding : BaseBinding<TextView, OverviewNodeState>
    {
        public TextViewAnswerStateTextColorBinding(TextView androidControl) : base(androidControl)
        {
        }

        protected override void SetValueToView(TextView control, OverviewNodeState value)
        {
            int? colorid = null;
            switch (value)
            {
                case OverviewNodeState.Answered:
                {
                    colorid = Resource.Color.recordedAnswerText;
                    break;
                }
                case OverviewNodeState.Unanswered:
                {
                    colorid = Resource.Color.disabledTextColor;
                    break;
                }
                case OverviewNodeState.Invalid:
                {
                    colorid = Resource.Color.errorTextColor;
                    break;
                }
                case OverviewNodeState.Commented:
                {
                    colorid = Resource.Color.commentsTextColor;
                    break;
                }
            }

            if (colorid.HasValue)
            {
                control.SetTextColor(new Color(ContextCompat.GetColor(control.Context, colorid.Value)));
            }
        }
    }
}
