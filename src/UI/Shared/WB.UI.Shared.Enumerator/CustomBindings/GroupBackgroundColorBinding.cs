﻿using Android.Widget;
using Cirrious.MvvmCross.Binding;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ButtonGroupStyleBinding : BaseBinding<Button, GroupStateViewModel>
    {
        public ButtonGroupStyleBinding(Button androidControl) : base(androidControl)
        {
        }

        public override MvxBindingMode DefaultMode
        {
            get { return MvxBindingMode.OneWay; }
        } 

        protected override void SetValueToView(Button control, GroupStateViewModel value)
        {
            SimpleGroupStatus status = value != null ? value.SimpleStatus : SimpleGroupStatus.Other;

            var groupBackgroundResourceId = GetGroupBackgroundResourceIdByStatus(status);

            control.SetBackgroundResource(groupBackgroundResourceId);
        }

        private static int GetGroupBackgroundResourceIdByStatus(SimpleGroupStatus? status)
        {
            switch (status)
            {
                case SimpleGroupStatus.Completed:
                    return Resource.Drawable.group_completed;

                case SimpleGroupStatus.Invalid:
                    return Resource.Drawable.group_with_invalid_answers;

                default:
                    return Resource.Drawable.group_started;
            }
        }
    }
}