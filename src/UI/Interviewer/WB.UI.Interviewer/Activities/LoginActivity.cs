using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Views;
using WB.UI.Shared.Enumerator.Activities;

namespace WB.UI.Interviewer.Activities
{
    [Activity(WindowSoftInputMode = SoftInput.StateHidden, Theme = "@style/GrayAppTheme")]
    public class LoginActivity : BaseActivity<LoginViewModel>
    {
        protected override int ViewResourceId
        {
            get { return Resource.Layout.Login; }
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            var toolbar = this.FindViewById<Toolbar>(Resource.Id.toolbar);
            toolbar.Title = "";
            this.SetSupportActionBar(toolbar);
        }

        public override void OnBackPressed()
        {
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            this.MenuInflater.Inflate(Resource.Menu.login, menu);

            menu.LocalizeMenuItem(Resource.Id.menu_settings, InterviewerUIResources.MenuItem_Title_Settings);

            menu.LocalizeMenuItem(Resource.Id.menu_diagnostics, InterviewerUIResources.MenuItem_Title_Diagnostics);
            return base.OnCreateOptionsMenu(menu);
        }
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.menu_settings:
                    Intent intent = new Intent(this, typeof(PrefsActivity));
                    this.StartActivity(intent);
                    break;
                case Resource.Id.menu_diagnostics:
                    this.ViewModel.NavigateToDiagnosticsPageCommand.Execute();
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }
    }
}