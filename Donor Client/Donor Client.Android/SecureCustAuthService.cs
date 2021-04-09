using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Browser.CustomTabs;
using DonorClient.Droid;
using DonorClient.Services;
using SKD.Common.Utils;
using Action = System.Action;
using Dependency = Xamarin.Forms.DependencyAttribute;

[assembly: Dependency(typeof(SecureCustAuthService))]
namespace DonorClient.Droid
{
    public class SecureCustAuthService : ISecureCustAuthService
    {
        private static bool isInitialised = false;

        public void Launch(string uri, Xamarin.Forms.Color accentColour)
            => OnLaunchRequested(uri, accentColour.ToSDColour().ToArgb());

        private static event Action<string, int> OnLaunchRequested;

        public static void Init(Activity activity)
        {
            if (isInitialised)
                return;

            OnLaunchRequested += (uri, accentColour) =>
            {
                var manager = CustomTabsActivityManager.From(activity);
                manager.CustomTabsServiceConnected += customTabServiceConnected;
                if (!manager.BindService())
                {
                    // Fall back to opening the system browser if necessary
                    var browserIntent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(uri));
                    activity.StartActivity(browserIntent);
                }

                void customTabServiceConnected(ComponentName name, CustomTabsClient client)
                {
                    manager.CustomTabsServiceConnected -= customTabServiceConnected;
                    var intent = new CustomTabsIntent.Builder()
                    .SetDefaultColorSchemeParams(new CustomTabColorSchemeParams.Builder()
                    .SetToolbarColor(accentColour)
                    .SetNavigationBarColor(accentColour)
                    .Build())
                    .Build();
                    intent.Intent.AddFlags(ActivityFlags.SingleTop | ActivityFlags.NoHistory | ActivityFlags.NewTask);
                    CustomTabsHelper.AddKeepAliveExtra(activity, intent.Intent);
                    intent.LaunchUrl(activity, Android.Net.Uri.Parse(uri));
                }

            };

            isInitialised = true;
        }
    }


    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Label = "Street Kids Direct", Icon = "@mipmap/icon", Theme = "@style/MainTheme.Launcher")]
    [IntentFilter(new[] { Intent.ActionView }, Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable }, DataScheme = "skd", DataHost = "sca.completed")]
    public class SCACustomTabActivity : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Finish();
        }
    }
}