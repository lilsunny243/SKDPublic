using System;
using System.Collections.Generic;
using System.Text;
using PartnerClient.Resources;
using SKD.Common.ViewModels;
using Xamarin.Forms;

namespace PartnerClient.ViewModels
{
    public class InvalidAuthViewModel : PageViewModel
    {

        private bool emailVerified;
        public bool EmailVerified { get => emailVerified; set => SetProperty(ref emailVerified, value, ResendCommand.ChangeCanExecute); }

        public Command LogoutCommand => new Command(() => Auth.SignOut());

        private Command? _resendCommand;
        public Command ResendCommand => _resendCommand ??= new Command(ExecuteResendCommand, () => !EmailVerified);

        private async void ExecuteResendCommand()
        {
            try
            {
                await Auth.CurrentUser!.SendEmailVerificationAsync(App.FirebaseActionCodeSettings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        public Command RefreshCommand => new Command((Shell.Current as AppShell)!.ForceOnIdTokenChanged);
    }
}
