using Plugin.FirebaseAuth;
using System;
using System.Threading.Tasks;

namespace DonorClient.Services
{
    public interface IAuthenticationService
    {
        public Task<string> GetFacebookAccessTokenAsync();
        public Task<AuthTokenResult> GetGoogleTokensAsync();
        public Task<AuthTokenResult> GetAppleTokensAsync();
        public void SignOut();

        /*
        public event EventHandler<AuthResultEventArgs> OnAuthResult;
        public event EventHandler<UserCollisionEventArgs> OnUserCollision;
        public event EventHandler<Exception> OnError;

        public void SignIn(AuthProvider provider, bool link);
        public void HandleException(Exception exception);
        */
    }

    public class AuthTokenResult
    {
        public AuthTokenResult(string idToken, string accessToken)
            => (IdToken, AccessToken) = (idToken, accessToken);
        public string IdToken { get; }
        public string AccessToken { get; }
    }

    //public class AuthResultEventArgs : EventArgs
    //{
    //    public AuthResultEventArgs(string accessToken, string idToken, AuthProvider provider)
    //        => (AccessToken, IdToken, Provider) = (accessToken, idToken, provider);

    //    public AuthResultEventArgs(IAuthCredential credential, AuthProvider provider)
    //        => (Credential, Provider) = (credential, provider);

    //    public readonly string AccessToken;
    //    public readonly string IdToken;
    //    public readonly IAuthCredential Credential;
    //    public readonly AuthProvider Provider;
    //}

    //public class UserCollisionEventArgs : EventArgs
    //{
    //    public UserCollisionEventArgs(string email, IAuthCredential credential)
    //        => (Email, Credential) = (email, credential);

    //    public readonly string Email;
    //    public readonly IAuthCredential Credential;
    //}

    public enum AuthProvider
    {
        Google,
        Microsoft,
        Apple,
        Facebook,
        Twitter,
        GitHub,
    }

}