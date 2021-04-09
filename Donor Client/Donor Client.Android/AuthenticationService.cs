using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Auth.Api;
using Android.Gms.Auth.Api.SignIn;
using Android.Runtime;
using DonorClient.Droid;
using DonorClient.Services;
using Xamarin.Facebook;
using Xamarin.Facebook.Login;
using Xamarin.Forms;

[assembly: Dependency(typeof(AuthenticationService))]
namespace DonorClient.Droid
{
    public class AuthenticationService : IAuthenticationService
    {
        private const int GoogleSignInRequestCode = 9001;
        private static bool isInitialised = false;
        private static ICallbackManager FacebookCallbackManager;

        private static event Action<TaskCompletionSource<string>> OnFacebookSignInRequestReceived;
        private static event Action<TaskCompletionSource<AuthTokenResult>> OnGoogleSignInRequestReceived;
        private static event Action<AuthenticationService> OnSignOutRequested;
        public static Action<int, Result, Intent> OnActivityResult;

        public AuthenticationService() => OnActivityResult += (requestCode, resultCode, data) =>
        {
            if (requestCode == GoogleSignInRequestCode)
            {
                var result = Auth.GoogleSignInApi.GetSignInResultFromIntent(data);
                if (result.IsSuccess)
                    googleIdTokenTCS?.SetResult(new AuthTokenResult(result.SignInAccount.IdToken, null));
                else
                    googleIdTokenTCS?.SetCanceled();
                googleIdTokenTCS = null;
            }
            else
                FacebookCallbackManager.OnActivityResult(requestCode, (int)resultCode, data);
        };

        public void SignOut() => OnSignOutRequested(this);

        public static void Init(Activity activity)
        {
            if (isInitialised)
                return;

            var gso = new GoogleSignInOptions.Builder(GoogleSignInOptions.DefaultSignIn)
                .RequestEmail()
                .RequestProfile()
                .RequestIdToken("") # REDACTED
                .Build();

            var gSignIn = GoogleSignIn.GetClient(activity, gso);

            OnGoogleSignInRequestReceived += tcs =>
            {
                var account = GoogleSignIn.GetLastSignedInAccount(activity);
                if (account?.IsExpired ?? true)
                    activity.StartActivityForResult(gSignIn.SignInIntent, GoogleSignInRequestCode);
                else
                    tcs.SetResult(new AuthTokenResult(account.IdToken, null));
            };

            FacebookCallbackManager = CallbackManagerFactory.Create();
            FacebookCallback loginCallback = null;
            OnFacebookSignInRequestReceived += tcs =>
            {
                if (loginCallback is null)
                {
                    loginCallback = new FacebookCallback(tcs);
                    LoginManager.Instance.RegisterCallback(FacebookCallbackManager, loginCallback);
                }

                AccessToken accessToken = AccessToken.CurrentAccessToken;
                if (accessToken?.IsExpired ?? true)
                    LoginManager.Instance.LogInWithReadPermissions(activity, new[] { "public_profile", "email" });
                else
                    tcs.SetResult(accessToken.Token);
            };

            OnSignOutRequested += async (service) =>
            {
                await gSignIn.SignOutAsync();
                LoginManager.Instance.LogOut();
            };

            isInitialised = true;

        }

        public Task<string> GetFacebookAccessTokenAsync()
        {
            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            OnFacebookSignInRequestReceived(tcs);
            return tcs.Task;
        }

        private TaskCompletionSource<AuthTokenResult>? googleIdTokenTCS;
        public Task<AuthTokenResult> GetGoogleTokensAsync()
        {
            googleIdTokenTCS = new TaskCompletionSource<AuthTokenResult>();
            OnGoogleSignInRequestReceived(googleIdTokenTCS);
            return googleIdTokenTCS.Task;
        }

        public Task<AuthTokenResult> GetAppleTokensAsync()
        {
            throw new InvalidOperationException("Integrated Apple Sign-In is not supported on Android");
        }
    }

    public class FacebookCallback: Java.Lang.Object, IFacebookCallback
    {
        private readonly TaskCompletionSource<string> _tcs;
        public FacebookCallback(TaskCompletionSource<string> tcs) => _tcs = tcs;

        public void OnCancel() => _tcs.SetCanceled();

        public void OnError(FacebookException ex) => _tcs.SetException(ex);

        public void OnSuccess(Java.Lang.Object result) => _tcs.SetResult(result.JavaCast<LoginResult>().AccessToken.Token);
    }

}
