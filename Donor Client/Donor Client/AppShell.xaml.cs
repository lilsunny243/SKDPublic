using System;
using System.Threading.Tasks;
using DonorClient.Models;
using DonorClient.Views;
using Plugin.FirebaseAuth;
using SKD.Common.Views;
using Xamarin.Forms;

namespace DonorClient
{
    public partial class AppShell : Shell
    {
        private static IAuth Auth => CrossFirebaseAuth.Current.Instance;

        private static bool isDonor;
        private static bool wasDonor;
        private bool isProcessingIdToken;

        public AppShell()
        {
            Routing.RegisterRoute("ProjectDetails", typeof(ProjectDetailPage));
            Routing.RegisterRoute("Settings", typeof(SettingsPage));
            Routing.RegisterRoute("QuestionnaireView", typeof(QuestionnaireViewPage));
            Routing.RegisterRoute("PastBundle", typeof(PastDonationBundlePage));
            InitializeComponent();
            if (Auth.CurrentUser is null)
                GoToAsync("//Login", false);
            DonorUser.CurrentChanged += u => _ = OnCurrentUserChanged(u);
            Auth.IdToken += OnIdTokenChanged;
        }

        private async void OnIdTokenChanged(object sender, IdTokenEventArgs e)
        {
            if (!isProcessingIdToken)
            {
                isProcessingIdToken = true;
                if (e.Auth.CurrentUser is null)
                    wasDonor = false;
                else
                {
                    var claims = (await e.Auth.CurrentUser.GetIdTokenResultAsync(false)).Claims;
                    bool isAdmin = false, isPartner = false;
                    if (claims.TryGetValue("admin", out var a))
                        isAdmin = (bool)a!;
                    if (claims.TryGetValue("partner", out var p))
                        isPartner = (bool)p!;
                    isDonor = !(isAdmin || isPartner);
                    if ((isDonor != wasDonor) || CurrentState.Location.OriginalString == "//Loading")
                        await OnCurrentUserChanged(DonorUser.Current);
                    wasDonor = isDonor;
                }
                DonorUser.SetCurrent(e.Auth.CurrentUser?.Uid);
                isProcessingIdToken = false;
            }
        }

        private async Task OnCurrentUserChanged(DonorUser? user)
        {
            string route = string.Empty;
            if (user is null)
                route = CurrentState.Location.OriginalString == "//Loading" ? "//Loading" : "//Login";
            else if (!isDonor)
                route = "//InvalidAuth";
            else if (!CurrentState.Location.OriginalString.StartsWith("//Home"))
                route = "//Home/Projects/Browse";

            if (!string.IsNullOrEmpty(route) && route != CurrentState.Location.OriginalString)
                await GoToAsync(route);
        }

        protected override void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
            if (args.Target?.Location.OriginalString == "//Login")
                LoginAppearing?.Invoke();
            if (args.Current?.Location.OriginalString == "//Login")
                LoginDisappearing?.Invoke();
        }

        public static event Action? LoginAppearing;
        public static event Action? LoginDisappearing;

    }
}
