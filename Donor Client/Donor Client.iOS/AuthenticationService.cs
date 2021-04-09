using DonorClient.iOS;
using DonorClient.Services;
using Facebook.CoreKit;
using Facebook.LoginKit;
using Firebase.Auth;
using Foundation;
using Plugin.FirebaseAuth;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using GSignIn = Google.SignIn.SignIn;

[assembly: Dependency(typeof(AuthenticationService))]
namespace DonorClient.iOS
{
    public class AuthenticationService : IAuthenticationService
    {
        private static bool isInitialised = false;

        private static event Action<TaskCompletionSource<string>> OnFacebookSignInRequestReceived;
        private static event Action<TaskCompletionSource<Services.AuthTokenResult>> OnGoogleSignInRequestReceived;
        private static event Action<TaskCompletionSource<Services.AuthTokenResult>> OnAppleSignInRequestReceived;
        private static event Action<AuthenticationService> OnSignOutRequested;

        public void SignOut() => OnSignOutRequested(this);
        public static void Init(AppDelegate appDelegate)
        {
            if (isInitialised)
                return;

            var gSignIn = GSignIn.SharedInstance;
            OnGoogleSignInRequestReceived += tcs =>
            {
                gSignIn.SignedIn += OnGoogleSignedIn;
                gSignIn.PresentingViewController ??= appDelegate.Window.RootViewController;
                gSignIn.ClientId ??= Firebase.Core.App.DefaultInstance?.Options.ClientId;
                if (gSignIn.HasPreviousSignIn)
                    gSignIn.RestorePreviousSignIn();
                else
                    gSignIn.SignInUser();

                void OnGoogleSignedIn(object sender, Google.SignIn.SignInDelegateEventArgs e)
                {
                    gSignIn.SignedIn -= OnGoogleSignedIn;
                    if (!(e.Error is null))
                        tcs.SetException(new NSErrorException(e.Error));
                    else if (e.User?.Authentication is null)
                        tcs.SetCanceled();
                    else
                        tcs.SetResult(new Services.AuthTokenResult(e.User.Authentication.IdToken, e.User.Authentication.AccessToken));
                }
            };

            OnAppleSignInRequestReceived += async tcs =>
            {
                if (DeviceInfo.Version.Major >= 13) // New Flow (Integrated)
                {
                    try
                    {
                        var appleResult = await AppleSignInAuthenticator.AuthenticateAsync(
                            new AppleSignInAuthenticator.Options() { IncludeEmailScope = true, IncludeFullNameScope = true });
                        if (appleResult is null)
                            tcs.SetCanceled();
                        else
                            tcs.SetResult(new Services.AuthTokenResult(appleResult.IdToken, appleResult.AccessToken));
                    }
                    catch(Exception ex) {
                        tcs.SetException(ex);
                    }

                }
                else throw new InvalidOperationException("Illegal attempt to peform integrated iOS sign in pre version 13");
            };

            var facebookLoginManager = new LoginManager();
            OnFacebookSignInRequestReceived += tcs =>
            {
                AccessToken accessToken = AccessToken.CurrentAccessToken;
                if (accessToken?.IsExpired ?? true)
                    facebookLoginManager.LogIn(new[] { "public_profile", "email" },
                        appDelegate.Window.RootViewController,
                        (result, error) =>
                        {
                            if (error is null)
                            {
                                if (result?.Token?.TokenString is null)
                                    tcs.SetCanceled();
                                else
                                    tcs.SetResult(result.Token.TokenString);
                            }
                            else
                                tcs.SetException(new NSErrorException(error));
                        });
                else
                    tcs.SetResult(accessToken.TokenString);
            };

            OnSignOutRequested += (service) =>
            {
                gSignIn.SignOutUser();
                facebookLoginManager.LogOut();
            };

            isInitialised = true;

        }

        public Task<string> GetFacebookAccessTokenAsync()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            OnFacebookSignInRequestReceived(tcs);
            return tcs.Task;
        }

        public Task<Services.AuthTokenResult> GetGoogleTokensAsync()
        {
            TaskCompletionSource<Services.AuthTokenResult> tcs = new TaskCompletionSource<Services.AuthTokenResult>();
            OnGoogleSignInRequestReceived(tcs);
            return tcs.Task;
        }

        public Task<Services.AuthTokenResult> GetAppleTokensAsync()
        {
            TaskCompletionSource<Services.AuthTokenResult> tcs = new TaskCompletionSource<Services.AuthTokenResult>();
            OnAppleSignInRequestReceived(tcs);
            return tcs.Task;
        }
    }
}