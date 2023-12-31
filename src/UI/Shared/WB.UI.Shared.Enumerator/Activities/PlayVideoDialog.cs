﻿#nullable enable

using System;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using MvvmCross;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.UI.Shared.Enumerator.Activities
{
    [MvxDialogFragmentPresentation]
    [Register(nameof(PlayVideoDialog))]
    public class PlayVideoDialog : BaseFragmentDialog<PlayVideoViewModel>
    {
        public PlayVideoDialog()
        {
        }

        protected PlayVideoDialog(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        protected override string? Title => ViewModel?.Title;
        protected override int LayoutFragmentId => Resource.Layout.play_video_dialog;
    }
}
