using Cirrious.MvvmCross.ViewModels;

namespace WB.Core.SharedKernels.Enumerator.ViewModels
{
    public abstract class BaseViewModel : MvxViewModel
    {
        public abstract void NavigateToPreviousViewModel();
    }
}