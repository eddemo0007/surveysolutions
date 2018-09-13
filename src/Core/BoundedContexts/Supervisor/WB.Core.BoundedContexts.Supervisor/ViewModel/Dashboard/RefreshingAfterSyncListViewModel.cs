﻿using MvvmCross.Plugin.Messenger;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.Dashboard;

namespace WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard
{
    public abstract class RefreshingAfterSyncListViewModel : ListViewModel
    {
        private IMvxMessenger messenger => ServiceLocator.Current.GetInstance<IMvxMessenger>();
        private MvxSubscriptionToken messengerSubscribtion;

        public override void ViewAppeared()
        {
            base.ViewAppeared();
            messengerSubscribtion = messenger.Subscribe<DashboardChangedMsg>(async msg => await this.UpdateUiItemsAsync(), MvxReference.Strong);
            UpdateUiItemsAsync().WaitAndUnwrapException();
        }

        public override void ViewDisappeared()
        {
            base.ViewDisappeared();
            messengerSubscribtion.Dispose();
        }
    }
}
