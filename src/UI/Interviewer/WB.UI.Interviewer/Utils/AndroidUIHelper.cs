using System;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Graphics;
using Android.Views;
using Android.Widget;

namespace WB.UI.Interviewer.Utils
{
    public static class AndroidUIHelper
    {
        public static CancellationTokenSource WaitForLongOperation(this Activity activity,
            Func<CancellationToken, Task> operation, 
            bool showProgressDialog = true)
        {
            var cancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = cancellationTokenSource.Token;

            var progressBar = showProgressDialog ? CreateProgressBar(activity) : null;

            Task.Factory.StartNew(
                () =>
                {
                    operation(cancellationToken).ContinueWith((t) =>
                    {
                        if (showProgressDialog)
                        {
                            activity.RunOnUiThread(() => { progressBar.Visibility = ViewStates.Gone; });
                        }

                    }, cancellationToken).Start(TaskScheduler.Current);
                }, cancellationToken);
            return cancellationTokenSource;
        }

        public static CancellationTokenSource WaitForLongOperation(this Activity activity,
           Action<CancellationToken> operation,
           bool showProgressDialog = true)
        {

            var cancellationTokenSource = new CancellationTokenSource();

            var cancellationToken = cancellationTokenSource.Token;

            var progressBar = showProgressDialog ? CreateProgressBar(activity) : null;

            Task.Factory.StartNew(
                () =>
                {
                    operation(cancellationToken);
                    if (showProgressDialog)
                    {
                        activity.RunOnUiThread(() => { progressBar.Visibility = ViewStates.Gone; });
                    }
                }, cancellationToken);
            return cancellationTokenSource;
        }

        private static ProgressBar CreateProgressBar(Activity activity)
        {
            var progressBar = new ProgressBar(activity);
            var display = activity.WindowManager.DefaultDisplay;
            var size = new Point();
            display.GetSize(size);
            progressBar.SetX(size.X / 2);
            progressBar.SetY(size.Y / 2);
            activity.AddContentView(progressBar,
                new ViewGroup.LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent));
            return progressBar;
        }
        
    }
}