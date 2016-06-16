using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Views;
using Android.Widget;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.UI.Shared.Enumerator.Activities;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace WB.UI.Interviewer.Activities
{
    [Activity(Label = "",
        Theme = "@style/GrayAppTheme",
        WindowSoftInputMode = SoftInput.StateHidden,
        ConfigurationChanges =
            Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    public class LoadingActivity : BaseActivity<LoadingViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.loading;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            this.SetSupportActionBar(toolbar);

            await this.ViewModel.RestoreInterviewAndNavigateThere();
            this.Finish();
        }

        public override async void OnBackPressed()
        {
            await this.ViewModel.NavigateToDashboardCommand.ExecuteAsync();
            this.CancelLoadingAndFinishActivity();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_dashboard:
                    this.ViewModel.NavigateToDashboardCommand.Execute();
                    this.CancelLoadingAndFinishActivity();
                    break;
                case Resource.Id.menu_signout:
                    this.ViewModel.SignOutCommand.Execute();
                    this.CancelLoadingAndFinishActivity();
                    break;
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    this.CancelLoadingAndFinishActivity();
                    break;
                case Android.Resource.Id.Home:
                    this.CancelLoadingAndFinishActivity();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.diagnostics, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_dashboard, InterviewerUIResources.MenuItem_Title_Dashboard);
            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);
            menu.LocalizeMenuItem(Resource.Id.menu_signout, InterviewerUIResources.MenuItem_Title_SignOut);

            this.HideMenuItem(menu, Resource.Id.menu_login);
            return base.OnCreateOptionsMenu(menu);
        }

        public void HideMenuItem(IMenu menu, int menuItemId)
        {
            var menuItem = menu.FindItem(menuItemId);
            menuItem?.SetVisible(false);
        }

        private void CancelLoadingAndFinishActivity()
        {
            this.ViewModel.CancelLoadingCommand.Execute();
            this.Finish();
        }
    }
}