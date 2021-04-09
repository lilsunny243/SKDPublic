using PartnerClient.Models;
using PartnerClient.Resources;
using PartnerClient.Services;
using PartnerClient.Views;
using Plugin.FirebaseAuth;
using SKD.Common.Services;
using SKD.Common.Views;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PartnerClient
{
    public partial class AppShell : Shell
    {
        private static IAuth Auth => CrossFirebaseAuth.Current.Instance;

        private static bool emailVerified, wasEmailVerified;
        private static bool isPartner, wasPartner;
        private static string? teamRole, previousTeamRole;
        private static string? teamUID, previousTeamUID;
        private bool isProcessingIdToken;

        public AppShell()
        {
            Routing.RegisterRoute("ProjectDetails", typeof(ProjectDetailPage));
            Routing.RegisterRoute("ProjectCreation", typeof(ProjectCreationPage));
            Routing.RegisterRoute("ChildDetails", typeof(ChildDetailPage));
            Routing.RegisterRoute("ProjectUpdateCreation", typeof(ProjectUpdateCreationPage));
            Routing.RegisterRoute("QuestionnaireView", typeof(QuestionnaireViewPage));
            Routing.RegisterRoute("QuestionnaireCreation", typeof(QuestionnaireCreationPage));
            Routing.RegisterRoute("Settings", typeof(SettingsPage));
            InitializeComponent();
            if (Auth.CurrentUser is null)
                GoToAsync("//Login", false);
            PartnerUser.CurrentUpdated += u => _ = OnCurrentUserUpdated(u);
            Auth.IdToken += OnIdTokenChanged;
        }

        public void ForceOnIdTokenChanged() => OnIdTokenChanged(new IdTokenEventArgs(Auth), true);
        private void OnIdTokenChanged(object sender, IdTokenEventArgs e) => OnIdTokenChanged(e, false);
        private async void OnIdTokenChanged(IdTokenEventArgs e, bool forceRefresh)
        {
            if (!isProcessingIdToken)
            {
                isProcessingIdToken = true;
                if (e.Auth.CurrentUser is null)
                    wasEmailVerified = wasPartner = false;
                else
                {
                    var claims = (await e.Auth.CurrentUser.GetIdTokenResultAsync(forceRefresh)).Claims;
                    if (claims.TryGetValue("partner", out var p))
                        isPartner = (bool)p!;
                    if (claims.TryGetValue("teamRole", out var tR))
                        teamRole = (string?)tR;
                    if (claims.TryGetValue("teamUID", out var tU))
                        teamUID = (string?)tU;
                    if (claims.TryGetValue("email_verified", out var eV))
                        emailVerified = (bool)eV!;
                    if ((emailVerified != wasEmailVerified)
                    || (isPartner != wasPartner)
                    || (teamUID != previousTeamUID)
                    || (teamRole != previousTeamRole)
                    || CurrentState.Location.OriginalString == "//Loading")
                        await OnCurrentUserUpdated(PartnerUser.Current);
                    (wasEmailVerified, wasPartner) = (emailVerified, isPartner);
                    (previousTeamUID, previousTeamRole) = (teamUID, teamRole);
                }
                PartnerUser.SetCurrent(e.Auth.CurrentUser?.Uid);
                isProcessingIdToken = false;
            }
        }

        private async Task OnCurrentUserUpdated(PartnerUser? user)
        {
            string route = string.Empty;
            if (user is null)
                route = CurrentState.Location.OriginalString == "//Loading" ? "//Loading" : "//Login";
            else if (!emailVerified)
                route = "//InvalidAuth?emailVerified=false";
            else if (!isPartner)
                route = "//InvalidAuth?emailVerified=true";
            else if (!user.TeamConfirmed)
                route = "//Onboarding";
            else if (!CurrentState.Location.OriginalString.StartsWith("//Home"))
                route = "//Home/Projects/List";

            bool isTeamLeader = teamRole is "leader" || teamRole is "requestedLeader";
            bool? isTeamConfirmed = !teamRole?.StartsWith("requested");
            if (!(user is null) && (teamUID != user.TeamUID || isTeamLeader != user.IsTeamLeader || isTeamConfirmed != user.TeamConfirmed))
                await Auth.CurrentUser!.GetIdTokenAsync(true);

            if (!string.IsNullOrEmpty(route) && route.Split('?')[0] != CurrentState.Location.OriginalString)
                await GoToAsync(route);

            if (!emailVerified && !(user?.EmailVerificationLinkSent ?? true))
            {
                try
                {
                    await Auth.CurrentUser!.SendEmailVerificationAsync(App.FirebaseActionCodeSettings);
                    await user.Doc.UpdateAsync(new { EmailVerificationLinkSent = true });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }
            }
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
