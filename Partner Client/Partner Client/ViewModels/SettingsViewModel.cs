using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using PartnerClient.Models;
using PartnerClient.Resources;
using PartnerClient.Utils;
using Plugin.FirebaseAuth;
using SKD.Common.Models;
using SKD.Common.Themes;
using SKD.Common.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Application = Xamarin.Forms.Application;
using DeviceInfo = Xamarin.Essentials.DeviceInfo;

namespace PartnerClient.ViewModels
{
    public class SettingsViewModel : PageViewModel
    {
        public SettingsViewModel()
        {
            Title = AppResources.Settings;
            userNameObservable = Observable
                .FromEvent<string>(x => UserNameChanged += x, x => UserNameChanged -= x)
                .Where(x => !string.IsNullOrEmpty(x) && x != PartnerUser.Current?.Name)
                .Throttle(TimeSpan.FromSeconds(2));
            PartnerUser.CurrentChanged += OnUserChanged;
            PartnerUser.CurrentUpdated += OnUserUpdated;
            OnUserChanged(PartnerUser.Current);
            OnUserUpdated(PartnerUser.Current);
        }

        #region Data
        private readonly IObservable<string> userNameObservable;
        private event Action<string>? UserNameChanged;
        private string userName = string.Empty;

        public string UserName { get => userName; set => SetProperty(ref userName, value, () => UserNameChanged?.Invoke(value)); }

        private void OnUserUpdated(PartnerUser? user)
        {
            if (user is null) return;
            UserName = user.Name;
            foreach (var vm in ThemeButtonViewModels)
                vm.IsSelected = vm.Param == user.DesiredTheme;
        }

        private IDisposable? userNameSubscription;
        private void OnUserChanged(PartnerUser? user)
        {
            userNameSubscription?.Dispose();
            if (!(user is null))
                userNameSubscription = userNameObservable.Subscribe(x => _ = user.Doc.UpdateAsync(new { Name = x }));

        }

        #endregion

        #region Auth

        public Command SignOutCommand => new Command(ExecuteSignOutCommand);

        private void ExecuteSignOutCommand()
        {
            Auth.SignOut();
        }


        public Command ChangePasswordCommand => new Command(ExecuteChangePasswordCommand);
        private async void ExecuteChangePasswordCommand()
        {
            try
            {
                var emailProvider = CrossFirebaseAuth.Current.EmailAuthProvider;
                var currentUser = Auth.CurrentUser!;
                var currentPassword = await Shell.Current.DisplayPromptAsync(AppResources.CurrentPassword, AppResources.CurrentPasswordPrompt, AppResources.Okay, AppResources.Cancel);
                if (string.IsNullOrWhiteSpace(currentPassword)) return;
                await currentUser.ReauthenticateAsync(emailProvider.GetCredential(currentUser.Email!, currentPassword));
                var newPassword = await Shell.Current.DisplayPromptAsync(AppResources.NewPassword, AppResources.NewPasswordPrompt, AppResources.Okay, AppResources.Cancel);
                if (string.IsNullOrWhiteSpace(newPassword)) return;
                if (PasswordHelpers.Evaulate(newPassword) == PasswordStrengthTier.Strong)
                    await Auth.CurrentUser!.UpdatePasswordAsync(newPassword);
                else
                    await Shell.Current.DisplayAlert(AppResources.PSWeak, AppResources.ChooseStrongerPassword, AppResources.Okay);
            }
            catch (Exception ex)
            {
                if (ex is FirebaseAuthException fex)
                {
                    Auth.SignOut();
                    await Shell.Current.DisplayAlert(AppResources.AuthError, fex.Message, AppResources.Okay);
                }
            }
        }


        #endregion

        #region Theme

        public ThemeButtonViewModel[] ThemeButtonViewModels { get; } = new[]
        {
            new ThemeButtonViewModel(Theme.System),
            new ThemeButtonViewModel(Theme.Light),
            new ThemeButtonViewModel(Theme.Dark)
        };

        public Command<Theme> ChangeThemeCommand => new Command<Theme>(ExecuteChangeThemeCommand);

        private async void ExecuteChangeThemeCommand(Theme theme)
        {
            foreach (ThemeButtonViewModel vm in ThemeButtonViewModels)
                vm.IsSelected = vm.Param == theme;
            await PartnerUser.Current!.Doc.UpdateAsync(new { DesiredTheme = theme.ToString() });
        }

        #endregion

    }

    public class ThemeButtonViewModel : SwitchButtonViewModel<Theme>
    {
        public ThemeButtonViewModel(Theme theme) : base(GetLocalisedName(theme), theme, PartnerUser.Current!.DesiredTheme == theme) { }

        private static string GetLocalisedName(Theme theme) => theme switch
        {
            Theme.Light => AppResources.Light,
            Theme.Dark => AppResources.Dark,
            Theme.System => AppResources.System,
            _ => string.Empty
        };
    }

}
