using PartnerClient.Models;
using PartnerClient.Resources;
using PartnerClient.Utils;
using Plugin.CloudFirestore;
using Plugin.FirebaseAuth;
using SKD.Common.Models;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace PartnerClient.ViewModels
{
    public class LoginViewModel : PageViewModel
    {

        private string email = string.Empty;
        private string password = string.Empty;
        private string passwordConfirmation = string.Empty;
        private bool signUp = true;
        private PasswordStrengthTier passwordStrength;

        public Command<bool> SwitchButtonCommand => new Command<bool>(b =>
        {
            SignUp = b;
            SwitchButtonItems[0].IsSelected = b;
            SwitchButtonItems[1].IsSelected = !b;
            AuthenticateCommand.ChangeCanExecute();
        });

        public List<SwitchButtonViewModel<bool>> SwitchButtonItems { get; } = new List<SwitchButtonViewModel<bool>>
        {
            new SwitchButtonViewModel<bool>(AppResources.SignUp, true, true),
            new SwitchButtonViewModel<bool>(AppResources.Login, false)
        };

        public bool SignUp { get => signUp; set => SetProperty(ref signUp, value); }

        public string Email { get => email; set => SetProperty(ref email, value, OnInputChanged, x => Regex.IsMatch(x, @"^[\w.@]*$")); }
        public string Password { get => password; set => SetProperty(ref password, value, OnPasswordInputChanged); }
        public string PasswordConfirmation { get => passwordConfirmation; set => SetProperty(ref passwordConfirmation, value, OnInputChanged); }
        public PasswordStrengthTier PasswordStrength { get => passwordStrength; set => SetProperty(ref passwordStrength, value); }

        public bool InputsValid => !string.IsNullOrWhiteSpace(Email) && email.Contains("@") && !string.IsNullOrWhiteSpace(Password)
            && (!SignUp || (Password == PasswordConfirmation && PasswordHelpers.Evaulate(Password) == PasswordStrengthTier.Strong));

        private void OnInputChanged()
        {
            OnPropertyChanged(nameof(InputsValid));
            AuthenticateCommand.ChangeCanExecute();
        }

        private void OnPasswordInputChanged()
        {
            OnPropertyChanged(nameof(InputsValid));
            AuthenticateCommand.ChangeCanExecute();
            if (!string.IsNullOrEmpty(Password)) // Keep it the same as it was before if it's empty so it doesn't change during fadeout
                PasswordStrength = PasswordHelpers.Evaulate(Password);
        }


        private bool isBusy;
        public bool IsBusy { get => isBusy; set => SetProperty(ref isBusy, value, AuthenticateCommand.ChangeCanExecute); }

        private Command? _authenticateCommand;
        public Command AuthenticateCommand => _authenticateCommand ??= new Command(ExecuteAuthenticateCommand, () => InputsValid && !IsBusy);

        private async void ExecuteAuthenticateCommand()
        {
            IsBusy = true;
            try
            {
                await (SignUp
                    ? Auth.CreateUserWithEmailAndPasswordAsync(Email, Password)
                    : Auth.SignInWithEmailAndPasswordAsync(Email, Password));
            }
            catch (Exception ex)
            {
                if(ex is FirebaseAuthException fex)
                {
                    if (fex.ErrorType == Plugin.FirebaseAuth.ErrorType.InvalidCredentials)
                    {
                        if (await Shell.Current.DisplayAlert(AppResources.ForgotPassword, string.Format(AppResources.ForgotPasswordPrompt, Email), AppResources.Okay, AppResources.Cancel))
                            await Auth.SendPasswordResetEmailAsync(Email);
                    }
                }
                else
                    await Shell.Current.DisplayAlert(AppResources.AuthError, ex.Message, AppResources.Okay);
            }
            finally
            {
                Email = Password = PasswordConfirmation = string.Empty;
                IsBusy = false;
            }
        }

    }
}
