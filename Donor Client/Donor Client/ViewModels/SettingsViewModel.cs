using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using AiForms.Dialogs;
using CreditCardValidator;
using DonorClient.Services;
using DonorClient.Utils;
using DonorClient.Views;
using Plugin.FirebaseAuth;
using SKD.Common.Models;
using SKD.Common.Services;
using SKD.Common.Themes;
using SKD.Common.Utils;
using SKD.Common.ViewModels;
using Stripe;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Application = Xamarin.Forms.Application;
using PaymentMethod = DonorClient.Models.PaymentMethod;
using DeviceInfo = Xamarin.Essentials.DeviceInfo;
using IListenerReg = Plugin.CloudFirestore.IListenerRegistration;
using DonorClient.Models;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace DonorClient.ViewModels
{
    public class SettingsViewModel : PageViewModel
    {
        public SettingsViewModel()
        {
            Title = "Settings";
            authService = DependencyService.Get<IAuthenticationService>();
            ClearPaymentData += () =>
            {
                InputCardNumber = InputCVC = InputExpiry = string.Empty;
                inputCardIssuer = CardIssuer.Unknown;
            };
            userNameObservable = Observable
                .FromEvent<string>(x => UserNameChanged += x, x => UserNameChanged -= x)
                .Where(x => !string.IsNullOrEmpty(x) && x != DonorUser.Current?.Name)
                .Throttle(TimeSpan.FromSeconds(2));
            DonorUser.CurrentChanged += OnUserChanged;
            DonorUser.CurrentUpdated += OnUserUpdated;
            OnUserChanged(DonorUser.Current);
            OnUserUpdated(DonorUser.Current);
        }

        #region Data
        private readonly IObservable<string> userNameObservable;
        private event Action<string>? UserNameChanged;
        private string userName = string.Empty;

        public string UserName { get => userName; set => SetProperty(ref userName, value, () => UserNameChanged?.Invoke(value)); }

        private void OnUserUpdated(DonorUser? user)
        {
            if (user is null) return;
            UserName = user.Name;
            GiftAidEnabled = user.GiftAidEnabled;
            foreach (var vm in ThemeButtonViewModels)
                vm.IsSelected = vm.Param == user.DesiredTheme;
        }

        private IListenerReg? paymentMethodListener;
        private IDisposable? userNameSubscription;
        private void OnUserChanged(DonorUser? user)
        {
            paymentMethodListener?.Remove();
            userNameSubscription?.Dispose();
            RefreshAuthProviders();
            if (!(user is null))
            {
                paymentMethodListener = FirestoreCollectionService.Subscribe<PaymentMethod, PaymentMethodViewModel>(user.Doc.Collection("PaymentMethods"), PaymentMethodViewModels, x => new PaymentMethodViewModel(x));
                userNameSubscription = userNameObservable.Subscribe(x => _ = user.Doc.UpdateAsync(new { Name = x }));
            }
        }

        #endregion

        #region Auth
        private IFirebaseAuth FirebaseAuth => CrossFirebaseAuth.Current;
        private readonly IAuthenticationService authService;

        public LinkableAuthButtonViewModel[] AuthProviderButtonViewModels { get; } = new[]
        {
            new LinkableAuthButtonViewModel(AuthProvider.Google),
            new LinkableAuthButtonViewModel(AuthProvider.Microsoft),
            new LinkableAuthButtonViewModel(AuthProvider.Apple),
            new LinkableAuthButtonViewModel(AuthProvider.Facebook),
            new LinkableAuthButtonViewModel(AuthProvider.Twitter),
            new LinkableAuthButtonViewModel(AuthProvider.GitHub)
        };

        private async void RefreshAuthProviders()
        {
            var user = Auth.CurrentUser;
            if (user is null) return;
            var providerIDs = await Auth.FetchSignInMethodsForEmailAsync(string.IsNullOrEmpty(user.Email) ? DonorUser.Current!.Email : user.Email!);
            var providers = providerIDs.Select(id => AuthProviderFromFirebaseId(id)).ToArray();
            Device.BeginInvokeOnMainThread(() =>
            {
                AuthProviderButtonViewModels.ForEach(x => x.IsLinked = providers.Contains(x.Provider));
                ToggleLinkAuthProviderCommand.ChangeCanExecute();
            });
        }

        private Command<LinkableAuthButtonViewModel>? _toggleLinkAuthProviderCommand;
        public Command<LinkableAuthButtonViewModel> ToggleLinkAuthProviderCommand 
            => _toggleLinkAuthProviderCommand ??= new Command<LinkableAuthButtonViewModel>(
                ExecuteToggleLinkAuthProviderCommand, 
                vm => !vm.IsLinked || AuthProviderButtonViewModels.Count(x => x.IsLinked) > 1);
        private async void ExecuteToggleLinkAuthProviderCommand(LinkableAuthButtonViewModel vm)
        {
            if (vm.IsLinked)
            {
                await Auth.CurrentUser!.UnlinkAsync(vm.Provider.GetFirebaseId());
                RefreshAuthProviders();
            }
            else
                await LinkAccountWithProviderAsync(vm.Provider);
        }

        private async Task LinkAccountWithProviderAsync(AuthProvider provider)
        {
            try
            {
                Task linkTask = provider switch
                {
                    AuthProvider.Facebook => LinkWithFacebookAsync(),
                    AuthProvider.Google => LinkWithGoogleAsync(),
                    AuthProvider.Apple when
                        DeviceInfo.Platform == DevicePlatform.iOS // For integrated sign in
                        && DeviceInfo.Version.Major >= 13
                        => LinkWithAppleIntegratedAsync(),
                    _ => Auth.CurrentUser!.LinkWithProviderAsync(provider.GetOAuthProvider())
                };
                await linkTask;
            }
            catch (Exception ex)
            {
                if (!(ex is TaskCanceledException))
                    _ = Shell.Current.DisplayAlert("Authentication Error", ex.Message, "Okay");
            }
            finally
            {
                RefreshAuthProviders();
            }
        }

        private async Task LinkWithFacebookAsync()
        {
            var accessToken = await authService.GetFacebookAccessTokenAsync();
            var credential = FirebaseAuth.FacebookAuthProvider.GetCredential(accessToken);
            await Auth.CurrentUser!.LinkWithCredentialAsync(credential);
        }

        private async Task LinkWithGoogleAsync()
        {
            var tokenResult = await authService.GetGoogleTokensAsync();
            var credential = FirebaseAuth.GoogleAuthProvider.GetCredential(tokenResult.IdToken, tokenResult.AccessToken);
            await Auth.CurrentUser!.LinkWithCredentialAsync(credential);
        }

        private async Task LinkWithAppleIntegratedAsync()
        {
            var tokenResult = await authService.GetAppleTokensAsync();
            var credential = FirebaseAuth.OAuthProvider.GetCredential("apple.com",
                tokenResult.IdToken, tokenResult.AccessToken);
            await Auth.CurrentUser!.LinkWithCredentialAsync(credential);
        }

        public Command SignOutCommand => new Command(ExecuteSignOutCommand);

        private void ExecuteSignOutCommand()
        {
            authService.SignOut();
            Auth.SignOut();
        }

        private AuthProvider AuthProviderFromFirebaseId(string id)
            => (AuthProvider)Enum.Parse(typeof(AuthProvider), id.Replace(".com", string.Empty), true);

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
            await DonorUser.Current!.Doc.UpdateAsync(new { DesiredTheme = theme.ToString() });
        }

        #endregion

        #region Payment Methods

        private string inputCardNumber = string.Empty;
        private string inputCVC = string.Empty;
        private string inputExpiryMonth = string.Empty;
        private string inputExpiryYear = string.Empty;
        private CardIssuer inputCardIssuer = CardIssuer.Unknown;

        public ObservableCollection<PaymentMethodViewModel> PaymentMethodViewModels { get; } = new ObservableCollection<PaymentMethodViewModel>();

        public string InputCardNumber { get => inputCardNumber; set => SetProperty(ref inputCardNumber, value, OnCreditCardNumberInputChanged, x => x.IsNumeric() && x.Length <= (x.IsAmex() ? 15 : 16)); }
        public string InputCVC { get => inputCVC; set => SetProperty(ref inputCVC, value, OnCreditCardInputChanged, x => x.IsNumeric() && x.Length <= (InputCardNumber.IsAmex() ? 4 : 3)); }
        public string InputExpiry
        {
            get => inputExpiryMonth + inputExpiryYear;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    inputExpiryMonth = inputExpiryYear = string.Empty;
                else if (value.Contains(","))
                {
                    var parts = value.Split(',');
                    SetProperty(ref inputExpiryMonth, parts[0], onlyIf: x => Regex.IsMatch(x, @"^((0[1-9]?)|(1[0-2]?))$"));
                    SetProperty(ref inputExpiryYear, parts[1], onlyIf: x => Regex.IsMatch(x, @"^([0-9]{0,2})$"));
                }
                else
                    SetProperty(ref inputExpiryMonth, value, onlyIf: x => Regex.IsMatch(x, @"^((0[1-9]?)|(1[0-2]?))$"));
                OnCreditCardInputChanged();
                OnPropertyChanged(); // In case we need to block an invalid input, the entry needs to know to revert!
            }
        }

        public static Action? ClearPaymentData { get; private set; }

        public ImageSource InputCardPreviewImageSource => PaymentMethod.GetIconImageSource(inputCardIssuer);

        public bool PaymentMethodInputValid => CardNotExpired && (InputCardNumber.Length, InputCVC.Length) == (InputCardNumber.IsAmex() ? (15, 4) : (16, 3)) && Luhn.CheckLuhn(InputCardNumber) && InputCardNumber.CreditCardBrandIgnoreLength().IsAccepted();
        private bool CardNotExpired => !string.IsNullOrEmpty(inputExpiryMonth) && !string.IsNullOrEmpty(inputExpiryYear) && (int.Parse(inputExpiryYear) > (DateTime.Today.Year % 100) || (int.Parse(inputExpiryYear) == (DateTime.Today.Year % 100) && int.Parse(inputExpiryMonth) > DateTime.Today.Month));

        private void OnCreditCardNumberInputChanged()
        {
            OnCreditCardInputChanged();
            var newIssuer = string.IsNullOrWhiteSpace(InputCardNumber) ? CardIssuer.Unknown : InputCardNumber.CreditCardBrandIgnoreLength();
            if (newIssuer != inputCardIssuer)
            {
                inputCardIssuer = newIssuer;
                OnPropertyChanged(nameof(InputCardPreviewImageSource));
            }
        }
        private void OnCreditCardInputChanged() => OnPropertyChanged(nameof(PaymentMethodInputValid));

        public Command AddPaymentMethodCommand => new Command(ExecuteAddPaymentMethodCommand);

        private async void ExecuteAddPaymentMethodCommand()
        {
            // Dialog returns true if accepted, false if cancelled (i.e. back button pressed)
            if (await Dialog.Instance.ShowAsync<CreditCardInputView>(this))
            {
                var paymentMethod = new PaymentMethod()
                {
                    LastFourDigits = InputCardNumber.Substring(InputCardNumber.Length - 4, 4),
                    ExpiryMonth = int.Parse(inputExpiryMonth),
                    ExpiryYear = int.Parse(inputExpiryYear) + 100 * (DateTime.Today.Year / 100),
                    Provider = inputCardIssuer,
                    StripeStatus = StripeStatus.Pending
                };
                var tokenOptions = new TokenCreateOptions()
                {
                    Card = new TokenCardOptions()
                    {
                        Number = InputCardNumber,
                        Cvc = InputCVC,
                        ExpMonth = paymentMethod.ExpiryMonth,
                        ExpYear = paymentMethod.ExpiryYear
                    }
                };
                var tokenService = new TokenService();
                var stripeToken = await tokenService.CreateAsync(tokenOptions);
                paymentMethod.StripeToken = stripeToken.Id;
                await DonorUser.Current!.Doc.Collection("PaymentMethods").AddAsync(paymentMethod);
            }
            ClearPaymentData?.Invoke();
        }

        #endregion

        #region Gift Aid

        private bool giftAidEnabled;
        private bool giftAidEnabledInput;
        private string giftAidTitleInput = string.Empty;
        private string giftAidFirstNameInput = string.Empty;
        private string giftAidLastNameInput = string.Empty;
        private bool giftAidInputHasUKAddress = true;
        private string giftAidHouseNumberNameInput = string.Empty;
        private string giftAidPostCodeInput = string.Empty;
        private string giftAidStreetInput = string.Empty;
        private string giftAidTownInput = string.Empty;

        public bool GiftAidEnabled { get => giftAidEnabled; set => SetProperty(ref giftAidEnabled, value); }

        public bool GiftAidEnabledInput { get => giftAidEnabledInput; set => SetProperty(ref giftAidEnabledInput, value, OnGiftAidInputChanged); }
        public string GiftAidTitleInput
        {
            get => giftAidTitleInput; set => SetProperty(ref giftAidTitleInput, value, OnGiftAidInputChanged,
                x => string.IsNullOrEmpty(x) || Regex.IsMatch(x, @"^\w{0,4}$"));
        }
        public string GiftAidFirstNameInput
        {
            get => giftAidFirstNameInput; set => SetProperty(ref giftAidFirstNameInput, value, OnGiftAidInputChanged,
                x => string.IsNullOrEmpty(x) || Regex.IsMatch(x, @"^\w{0,35}$"));
        }
        public string GiftAidLastNameInput
        {
            get => giftAidLastNameInput; set => SetProperty(ref giftAidLastNameInput, value, OnGiftAidInputChanged,
                x => string.IsNullOrEmpty(x) || Regex.IsMatch(x, @"^[\w ]{0,35}$"));
        }
        public bool GiftAidInputHasUKAddress { get => giftAidInputHasUKAddress; set => SetProperty(ref giftAidInputHasUKAddress, value, OnGiftAidInputChanged); }
        private bool GiftAidAddressHasRemainingSpace => GiftAidInputHasUKAddress ||
            ((GiftAidHouseNumberNameInput?.Length ?? 0)
            + (GiftAidStreetInput?.Length ?? 0)
            + (GiftAidTownInput?.Length ?? 0) < 37);
        public string GiftAidHouseNumberNameInput
        {
            get => giftAidHouseNumberNameInput; set => SetProperty(ref giftAidHouseNumberNameInput, value, OnGiftAidInputChanged,
                x => string.IsNullOrEmpty(x) || Regex.IsMatch(x, @"^[\w ]{0,40}$") && GiftAidAddressHasRemainingSpace);
        }
        public string GiftAidPostCodeInput
        {
            get => giftAidPostCodeInput; set => SetProperty(ref giftAidPostCodeInput, value.ToUpper(), OnGiftAidInputChanged,
                x => string.IsNullOrEmpty(x) || Regex.IsMatch(x, @"^[A-Z0-9 ]{0,8}$"));
        }
        public string GiftAidStreetInput
        {
            get => giftAidStreetInput; set => SetProperty(ref giftAidStreetInput, value, OnGiftAidInputChanged,
                x => string.IsNullOrEmpty(x) || GiftAidAddressHasRemainingSpace);
        }
        public string GiftAidTownInput
        {
            get => giftAidTownInput; set => SetProperty(ref giftAidTownInput, value, OnGiftAidInputChanged,
                x => string.IsNullOrEmpty(x) || GiftAidAddressHasRemainingSpace);
        }

        private void OnGiftAidInputChanged() => OnPropertyChanged(nameof(GiftAidInputValid));

        public bool GiftAidInputValid => !GiftAidEnabledInput || (
            !string.IsNullOrEmpty(GiftAidTitleInput) &&
            !string.IsNullOrEmpty(GiftAidFirstNameInput) &&
            !string.IsNullOrEmpty(GiftAidLastNameInput) &&
            !string.IsNullOrEmpty(GiftAidHouseNumberNameInput) &&
            (!GiftAidInputHasUKAddress || (!string.IsNullOrEmpty(GiftAidPostCodeInput) && Regex.IsMatch(GiftAidPostCodeInput, @"^[A-Z][A-Z0-9]{1,3}\s?\d[A-Z]{2}$"))) &&
            (GiftAidInputHasUKAddress || (!string.IsNullOrEmpty(GiftAidStreetInput) && !string.IsNullOrEmpty(GiftAidTownInput))));
        
        public Command EditGiftAidCommand => new Command(ExecuteEditGiftAidCommand);
        private async void ExecuteEditGiftAidCommand()
        {
            var details = DonorUser.Current!.GiftAidDetails;
            GiftAidEnabledInput = GiftAidEnabled;
            GiftAidTitleInput = details?.Title ?? string.Empty;
            GiftAidFirstNameInput = details?.FirstName ?? string.Empty;
            GiftAidLastNameInput = details?.LastName ?? string.Empty;
            GiftAidInputHasUKAddress = !string.IsNullOrEmpty(details?.PostCode);
            GiftAidHouseNumberNameInput = details?.HouseNumberName ?? string.Empty;
            GiftAidPostCodeInput = details?.PostCode ?? string.Empty;
            GiftAidStreetInput = details?.Street ?? string.Empty;
            GiftAidTownInput = details?.Town ?? string.Empty;
            if (await Dialog.Instance.ShowAsync<GiftAidDetailsView>(this))
            {
                var newDetails = GiftAidEnabledInput ? new GiftAidUserDetails()
                {
                    Title = GiftAidTitleInput,
                    FirstName = GiftAidFirstNameInput,
                    LastName = GiftAidLastNameInput,
                    HouseNumberName = GiftAidHouseNumberNameInput,
                    PostCode = GiftAidInputHasUKAddress ? GiftAidPostCodeInput : null,
                    Street = GiftAidInputHasUKAddress ? null : GiftAidStreetInput,
                    Town = GiftAidInputHasUKAddress ? null : GiftAidTownInput
                } : null;
                await DonorUser.Current.Doc.UpdateAsync(new { GiftAidEnabled = GiftAidEnabledInput, GiftAidDetails = newDetails });
            }
        }

        #endregion
    }

    public class ThemeButtonViewModel : SwitchButtonViewModel<Theme>
    {
        public ThemeButtonViewModel(Theme theme) : base(theme.ToString(), theme, DonorUser.Current!.DesiredTheme == theme) { }
    }

    public class PaymentMethodViewModel : CardViewModel<PaymentMethod>
    {
        private readonly int expiryMonth;
        private readonly int expiryYear;
        private StripeStatus status;
        private bool isExpired;

        public ImageSource? IconImageSource { get; private set; }
        public bool IsExpired { get => isExpired; set => SetProperty(ref isExpired, value, RaisePropertyChanged(nameof(ExpiryString))); }
        public string LastFourDigits { get; private set; } = "xxxx";
        public string ExpiryString => $"{(IsExpired ? "Expired" : "Expires")} {months[expiryMonth - 1]} {expiryYear}";
        public StripeStatus Status { get => status; set => SetProperty(ref status, value); }

        public PaymentMethodViewModel(PaymentMethod pm) : base(pm)
            => (expiryMonth, expiryYear, IsExpired, LastFourDigits, IconImageSource, Status)
            = (pm.ExpiryMonth, pm.ExpiryYear, pm.IsExpired, pm.LastFourDigits,
            PaymentMethod.GetIconImageSource(pm.Provider), pm.StripeStatus);
        

        public override void Update(PaymentMethod pm)
        {
            bool didNotRequireAction = Status != StripeStatus.RequiresAction;
            base.Update(pm);
            IsExpired = pm.IsExpired;
            Status = pm.StripeStatus;
            bool doesRequireAction = pm.StripeStatus == StripeStatus.RequiresAction;
            if (didNotRequireAction && doesRequireAction)
                ExecuteAuthoriseCommand();
        }

        public Command AuthoriseCommand => new Command(ExecuteAuthoriseCommand);

        private void ExecuteAuthoriseCommand()
        {
            DependencyService.Get<ISecureCustAuthService>()
                .Launch(Source!.StripeRedirectUrl!, (Color)Application
                .Current.Resources["ChromeBackgroundColour"
                + (ThemeEngine.IsEffectivelyLight ? "Light" : "Dark")]);
        }

        public Command DeleteCommand => new Command(ExecuteDeleteCommand);

        private async void ExecuteDeleteCommand()
            => await DonorUser.Current!.Doc
            .Collection("PaymentMethods")
            .Document(Source!.UID)
            .DeleteAsync();

        private static readonly ReadOnlyCollection<string> months = new ReadOnlyCollection<string>(new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" });

    }

    public class LinkableAuthButtonViewModel : BaseViewModel
    {

        private bool isLinked;
        public bool IsLinked { get => isLinked; set => SetProperty(ref isLinked, value); }

        public LinkableAuthButtonViewModel(AuthProvider provider)
            => (Provider, Text, Glyph) = (provider, provider.ToString(), (string)Application.Current.Resources[provider.ToString() + "Icon"]);

        public AuthProvider Provider { get; }
        public string Text { get; }
        public string Glyph { get; }
    }

}
