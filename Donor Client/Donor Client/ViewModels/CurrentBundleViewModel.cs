using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AiForms.Dialogs;
using DonorClient.Models;
using DonorClient.Services;
using DonorClient.Views;
using Plugin.CloudFirestore;
using SKD.Common.Models;
using SKD.Common.Services;
using SKD.Common.Themes;
using Xamarin.Forms;

namespace DonorClient.ViewModels
{
    public class CurrentBundleViewModel : BaseDonationBundleViewModel
    {
        private readonly ISecureCustAuthService SecureCustAuthService;
        private readonly IApplePayService? ApplePayService;
        private readonly IGooglePayService? GooglePayService;

        private PaymentMethodViewModel? selectedPaymentMethodViewModel;
        private bool confirmed;
        private bool canUseGooglePay;

        public ObservableCollection<PaymentMethodViewModel> PaymentMethodViewModels { get; } = new ObservableCollection<PaymentMethodViewModel>();
        public PaymentMethodViewModel? SelectedPaymentMethodViewModel
        {
            get => selectedPaymentMethodViewModel;
            set => SetProperty(ref selectedPaymentMethodViewModel, value, RaisePropertyChanged(nameof(CanConfirmCard)));
        }

        public bool CanUseApplePay => ApplePayService?.GetIsSupported() ?? false;
        public bool CanUseGooglePay { get => canUseGooglePay; private set => SetProperty(ref canUseGooglePay, value); }

        public bool Confirmed { get => confirmed; set => SetProperty(ref confirmed, value, () => { OnPropertyChanged(nameof(CanConfirm)); SelectCardCommand.ChangeCanExecute(); }); }

        private Command? _selectCardCommand;
        public Command SelectCardCommand => _selectCardCommand ??= new Command(ExecuteSelectCardCommand, () => CanConfirm);

        public bool CanConfirm => Amount > 0 && !confirmed;
        public bool CanConfirmCard => !(SelectedPaymentMethodViewModel is null) && PaymentMethodViewModels.Any(x => x.UID == SelectedPaymentMethodViewModel.UID);


        private Command? _authorisePaymentCommand;
        public Command AuthorisePaymentCommand => _authorisePaymentCommand ??= new Command(ExecuteAuthorisePaymentCommand, () => Status is StripeStatus.RequiresAction && !string.IsNullOrEmpty(DonationBundle.Current?.StripeRedirectUrl));

        public Command SettingsCommand => new Command(ExecuteSettingsCommand);
        private async void ExecuteSettingsCommand() => await Shell.Current.GoToAsync("Settings");

        public Command<Donation> RemoveDonationCommand => new Command<Donation>(async (donation) => await DonationBundle.Current!.RemoveDonationAsync(donation));

        private IListenerRegistration? paymentMethodListener;
        public CurrentBundleViewModel()
        {
            Title = "Donation Bundle";
            SecureCustAuthService = DependencyService.Get<ISecureCustAuthService>();
            ApplePayService = DependencyService.Get<IApplePayService>();
            GooglePayService = DependencyService.Get<IGooglePayService>();
            GooglePayService?.GetIsSupportedAsync().ContinueWith(t => CanUseGooglePay = t.Result);
            DonationBundle.CurrentChanged += OnBundleChanged;
            DonationBundle.CurrentUpdated += OnBundleUpdated;
            DonorUser.CurrentChanged += OnUserChanged;
            DonorUser.CurrentUpdated += OnUserUpdated;
            OnBundleChanged(DonationBundle.Current);
            OnBundleUpdated(DonationBundle.Current);
            OnUserChanged(DonorUser.Current);
            OnUserUpdated(DonorUser.Current);
        }

        protected override void OnBundleChanged(DonationBundle? bundle)
        {
            base.OnBundleChanged(bundle);
            SelectedPaymentMethodViewModel = null;
        }

        protected override void OnBundleUpdated(DonationBundle? bundle)
        {
            base.OnBundleUpdated(bundle);
            OnPropertyChanged(nameof(CanConfirm));
            SelectCardCommand.ChangeCanExecute();
            AuthorisePaymentCommand.ChangeCanExecute();
            if (!(bundle is null))
            {
                Confirmed |= bundle.UserConfirmed;
                if (bundle.StripeStatus == StripeStatus.RequiresAction && !string.IsNullOrEmpty(bundle.StripeRedirectUrl))
                    ExecuteAuthorisePaymentCommand();
            }
        }

        private void OnUserUpdated(DonorUser? user)
        {
            GiftAidEnabled = user?.GiftAidEnabled ?? false;
            OnPropertyChanged(nameof(TotalReceived));
        }

        private void OnUserChanged(DonorUser? user)
        {
            paymentMethodListener?.Remove();
            if (!(user is null))
            {
                paymentMethodListener = FirestoreCollectionService.Subscribe<PaymentMethod, PaymentMethodViewModel>(
                    user.Doc.Collection("PaymentMethods").WhereEqualsTo(nameof(PaymentMethod.StripeStatus), StripeStatus.Succeeded.GetStringValue()),
                    PaymentMethodViewModels, x => new PaymentMethodViewModel(x), RaisePropertyChanged(nameof(CanConfirmCard)), x => !x.Source.IsExpired);
            }
        }

        private async Task<bool> ReconfirmDonationsAsync()
        {
            var projectsCollection = Firestore.Collection("Projects");
            var projects = (await Task.WhenAll(DonationViewModels.Select(x => projectsCollection
                .Document(x.Source!.ProjectUID).GetAsync())))
                .Select(x => x.ToObject<Project>());
            if (projects.Any(x => x!.Raised >= x.Target || x.Status == ProjectStatus.Completed))
                return await Shell.Current.DisplayAlert("Attention!", "Some of the projects in your selection " +
                    "have either reached their target or are marked as completed, do you wish to continue?", "Yes", "Cancel");
            return true;
        }

        public Command ApplePayCommand => new Command(ExecuteApplePayCommand);
        private async void ExecuteApplePayCommand()
        {
            try {
                if (await ReconfirmDonationsAsync() && CanUseApplePay)
                    await DonationBundle.ConfirmCurrentWithOneTimeTokenAsync(await ApplePayService!.GetTokenAsync(TotalPayed, DonationViewModels.Select(x => x.Source), CoverStripeFee), PaymentMethodType.ApplePay, CoverStripeFee);
                else
                    ConfirmationCancelled?.Invoke();
            }
            catch (Exception ex)
            {
                ConfirmationCancelled?.Invoke();
                Debug.WriteLine(ex.Message);
            }
        }

        public Command GooglePayCommand => new Command(ExecuteGooglePayCommand);
        private async void ExecuteGooglePayCommand()
        {
            try
            {
                if (await ReconfirmDonationsAsync() && CanUseGooglePay)
                    await DonationBundle.ConfirmCurrentWithOneTimeTokenAsync(await GooglePayService!.GetTokenAsync(TotalPayed), PaymentMethodType.GooglePay, CoverStripeFee);
                else
                    ConfirmationCancelled?.Invoke();
            }
            catch (Exception ex)
            {
                ConfirmationCancelled?.Invoke();
                Debug.WriteLine(ex.Message);
            }
        }


        public event Action? ConfirmationCancelled;
        private async void ExecuteSelectCardCommand()
        {
            try
            {
                if (await ReconfirmDonationsAsync() && await Dialog.Instance.ShowAsync<PaymentMethodSelectionView>(this))
                    await DonationBundle.ConfirmCurrentWithCardAsync(SelectedPaymentMethodViewModel!.Source!, CoverStripeFee);
                else
                    ConfirmationCancelled?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public bool AllowStatusUpdate
        {
            get => allowStatusUpdate; set
            {
                allowStatusUpdate = value;
                if (value)
                    OnBundleUpdated(DonationBundle.Current);
            }
        }

        private void ExecuteAuthorisePaymentCommand()
        {
            SecureCustAuthService.Launch(DonationBundle.Current!.StripeRedirectUrl!,
                (Color)Application.Current.Resources["ChromeBackgroundColour"
                + (ThemeEngine.IsEffectivelyLight ? "Light" : "Dark")]);
        }
    }

}
