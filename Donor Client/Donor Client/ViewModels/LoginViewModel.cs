using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DonorClient.Services;
using DonorClient.Utils;
using DonorClient.Views;
using Newtonsoft.Json;
using Plugin.FirebaseAuth;
using SKD.Common.Models;
using SKD.Common.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DonorClient.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {

        public LoginButtonViewModel[] ButtonViewModels { get; } = new[]
        {
            new LoginButtonViewModel(AuthProvider.Google, ImpactMeasurementArea.ChildHealth),
            new LoginButtonViewModel(AuthProvider.Apple, ImpactMeasurementArea.ResponsibleLimits),
            new LoginButtonViewModel(AuthProvider.Microsoft, ImpactMeasurementArea.Education),
            new LoginButtonViewModel(AuthProvider.Twitter, ImpactMeasurementArea.SocialIdentity),
            new LoginButtonViewModel(AuthProvider.Facebook, ImpactMeasurementArea.ConstructiveUseOfTime),
            new LoginButtonViewModel(AuthProvider.GitHub, ImpactMeasurementArea.SelfEsteemAndDreams)
        };

        private IFirebaseAuth FirebaseAuth => CrossFirebaseAuth.Current;
        private AuthProviderSelectionView? userCollisionSelectionView;

        private readonly IAuthenticationService authService;
        private IAuthCredential? pendingCredential;

        public Command AuthenticateCommand => new Command<AuthProvider>(ExecuteAuthenticateCommand);
        public Command<AuthProvider> AuthenticateWithPendingCredentialCommand 
            => new Command<AuthProvider>(ExecuteAuthenticateWithPendingCredentialCommand);

        public LoginViewModel()
        {
            authService = DependencyService.Get<IAuthenticationService>();
            Auth.AuthState += OnAuthStateChanged;
        }

        private async void OnAuthStateChanged(object sender, AuthStateEventArgs e)
        {
            if (!(e.Auth.CurrentUser is null) && (!(pendingCredential is null)))
            {
                await e.Auth.CurrentUser.LinkWithCredentialAsync(pendingCredential);
                pendingCredential = null;
            }
        }

        private async void HandleUserCollision(FirebaseAuthException ex)
        {
            pendingCredential = ex.UpdatedCredential;
            var providerIDs = await Auth.FetchSignInMethodsForEmailAsync(ex.Email!);
            var providers = providerIDs.Select(id => AuthProviderFromFirebaseId(id));
            await AiForms.Dialogs.Dialog.Instance.ShowAsync(userCollisionSelectionView = new AuthProviderSelectionView(
                ButtonViewModels.Where(x => providers.Contains(x.Provider)),
                AuthProviderFromFirebaseId(pendingCredential!.Provider)), this);
        }

        private void ExecuteAuthenticateCommand(AuthProvider provider)
        {
            pendingCredential = null;
            _ = SignInAsync(provider);
        }

        private void ExecuteAuthenticateWithPendingCredentialCommand(AuthProvider provider)
        {
            userCollisionSelectionView?.DialogNotifier.Complete();
            _ = SignInAsync(provider);
        }

        private async Task SignInAsync(AuthProvider provider)
        {
            try
            {
                Task signInTask = provider switch
                {
                    AuthProvider.Facebook => SignInWithFacebookAsync(),
                    AuthProvider.Google => SignInWithGoogleAsync(),
                    AuthProvider.Apple when
                        DeviceInfo.Platform == DevicePlatform.iOS // For integrated sign in
                        && DeviceInfo.Version.Major >= 13
                        => SignInWithAppleIntegratedAsync(),
                    _ => Auth.SignInWithProviderAsync(provider.GetOAuthProvider())
                };
                await signInTask;
            }
            catch (Exception ex)
            {
                if (ex is FirebaseAuthException fex && fex.ErrorType == ErrorType.UserCollision)
                    HandleUserCollision(fex);
                else if (!(ex is TaskCanceledException))
                    await Shell.Current.DisplayAlert("Authentication Error", ex.Message, "Okay");
            }
        }

        private async Task SignInWithFacebookAsync()
        {
            var accessToken = await authService.GetFacebookAccessTokenAsync();
            var credential = FirebaseAuth.FacebookAuthProvider.GetCredential(accessToken);
            await Auth.SignInWithCredentialAsync(credential);
        }

        private async Task SignInWithGoogleAsync()
        {
            var tokenResult = await authService.GetGoogleTokensAsync();
            var credential = FirebaseAuth.GoogleAuthProvider.GetCredential(tokenResult.IdToken, tokenResult.AccessToken);
            await Auth.SignInWithCredentialAsync(credential);
        }

        private async Task SignInWithAppleIntegratedAsync()
        {
            var tokenResult = await authService.GetAppleTokensAsync();
            var credential = FirebaseAuth.OAuthProvider.GetCredential("apple.com",
                tokenResult.IdToken, tokenResult.AccessToken);
            await Auth.SignInWithCredentialAsync(credential);
        }

        private AuthProvider AuthProviderFromFirebaseId(string id)
            => (AuthProvider)Enum.Parse(typeof(AuthProvider), id.Replace(".com", string.Empty), true);
    }


    public class LoginButtonViewModel
    {
        public LoginButtonViewModel(AuthProvider provider, ImpactMeasurementArea accentColourSource)
            => (Provider, Text, Glyph, AccentColour) = (provider, provider.ToString(), (string)Application.Current.Resources[provider.ToString() + "Icon"], accentColourSource.GetAccentColour());

        public AuthProvider Provider { get; }
        public string Text { get; }
        public string Glyph { get; }
        public Color AccentColour { get; }
    }

}
