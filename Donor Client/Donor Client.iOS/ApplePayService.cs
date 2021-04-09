using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DonorClient.iOS;
using DonorClient.Services;
using Foundation;
using UIKit;
using Xamarin.Forms;
using Stripe.iOS;
using PassKit;
using DonorClient.Models;
using ObjCRuntime;
using System.Threading.Tasks;

[assembly: Dependency(typeof(ApplePayService))]
namespace DonorClient.iOS
{
    public class ApplePayService : IApplePayService
    {
        private static bool isInitialised = false;
        private static event Action<TaskCompletionSource<string>, int, IEnumerable<Donation>, bool> OnGetTokenRequestReceived;

        public static void Init(AppDelegate appDelegate)
        {
            if (isInitialised)
                return;

            OnGetTokenRequestReceived += (tcs, totalToPay, donations, coverStripeFee) =>
            {
                var summaryItems = donations.Select(x => new PKPaymentSummaryItem()
                {
                    Label = x.ProjectNameEn,
                    Amount = new NSDecimalNumber(x.Amount / 100d)
                }).ToList();
                if (coverStripeFee)
                {
                    summaryItems.Add(new PKPaymentSummaryItem()
                    {
                        Label = "Stripe Processing Fee",
                        Amount = new NSDecimalNumber(Math.Ceiling((totalToPay * 0.014d) + 20d) / 100d)
                    });
                }
                summaryItems.Add(new PKPaymentSummaryItem()
                {
                    Label = "Street Kids Direct",
                    Amount = new NSDecimalNumber(totalToPay / 100d)
                });
                var paymentRequest = new PKPaymentRequest
                {
                    MerchantIdentifier = App.AppleMerchantId,
                    CountryCode = "GB",
                    CurrencyCode = "GBP",
                    SupportedNetworks = supportedNetworks,
                    PaymentSummaryItems = summaryItems.ToArray(),
                    MerchantCapabilities = PKMerchantCapability.ThreeDS,
                };
                var viewController = new PKPaymentAuthorizationViewController(paymentRequest)
                {
                    Delegate = new PKPaymentDelegateThing(tcs)
                };
                appDelegate.Window.RootViewController.PresentViewController(viewController, true, null);
            };

            isInitialised = true;
        }

        public bool GetIsSupported() => PKPaymentAuthorizationViewController.CanMakePaymentsUsingNetworks(supportedNetworks);

        public Task<string> GetTokenAsync(int totalToPay, IEnumerable<Donation> donations, bool coverStripeFee)
        {
            var tcs = new TaskCompletionSource<string>();
            OnGetTokenRequestReceived?.Invoke(tcs, totalToPay, donations, coverStripeFee);
            return tcs.Task;
        }

        private static readonly NSString[] supportedNetworks =
        {
            PKPaymentNetwork.Amex,
            PKPaymentNetwork.MasterCard,
            PKPaymentNetwork.Visa
        };
    }

    class PKPaymentDelegateThing : NSObject, IPKPaymentAuthorizationViewControllerDelegate
    {
        private readonly TaskCompletionSource<string> _tcs;
        public PKPaymentDelegateThing(TaskCompletionSource<string> tcs) => _tcs = tcs;

        public void DidAuthorizePayment(PKPaymentAuthorizationViewController controller, PKPayment payment, Action<PKPaymentAuthorizationStatus> completion)
        {
            ApiClient.SharedClient.CreateToken(payment, (token, error) =>
            {
                if (token is null)
                    _tcs.SetException(new NSErrorException(error));
                else
                    _tcs.SetResult(token.TokenId);
            });

            controller.DismissViewController(true, null);
        }

        public void PaymentAuthorizationViewControllerDidFinish(PKPaymentAuthorizationViewController controller)
        {
            _tcs.SetCanceled();
            controller.DismissViewController(true, null);
        }

        public void WillAuthorizePayment(PKPaymentAuthorizationViewController controller) { }
    }
}