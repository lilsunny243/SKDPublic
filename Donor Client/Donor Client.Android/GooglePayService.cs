using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Gms.Wallet;
using DonorClient.Droid;
using DonorClient.Services;
using Newtonsoft.Json;
using Xamarin.Forms;

[assembly: Dependency(typeof(GooglePayService))]
namespace DonorClient.Droid
{
    public class GooglePayService : IGooglePayService
    {
        private static bool isInitialised = false;
        private const int PaymentDataRequestCode = 6969;

        private static event Action<TaskCompletionSource<bool>> OnIsSupportedRequestReceived;
        private static event Action<TaskCompletionSource<string>, int> OnGetTokenRequestReceived;
        public static Action<int, Result, Intent> OnActivityResult;

        public GooglePayService() => OnActivityResult += (requestCode, resultCode, data) =>
        {
            if (requestCode == PaymentDataRequestCode)
            {
                if (resultCode == Result.Ok)
                {
                    var paymentData = PaymentData.GetFromIntent(data);
                    var tokenData = paymentData.PaymentMethodToken.Token;
                    var tokenObj = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(tokenData);
                    tokenTCS.SetResult(tokenObj.GetValue("id").ToObject<string>());
                }
                else if (resultCode == Result.Canceled)
                    tokenTCS.SetCanceled();
                tokenTCS = null;
            }
        };

        public Task<bool> GetIsSupportedAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            OnIsSupportedRequestReceived(tcs);
            return tcs.Task;
        }

        private TaskCompletionSource<string>? tokenTCS;
        public Task<string> GetTokenAsync(int totalToPay)
        {
            tokenTCS = new TaskCompletionSource<string>();
            OnGetTokenRequestReceived(tokenTCS, totalToPay);
            return tokenTCS.Task;
        }

        public static void Init(Activity activity)
        {
            if (isInitialised)
                return;

            var paymentsClient = WalletClass.GetPaymentsClient(activity,
                new WalletClass.WalletOptions.Builder()
#if DEBUG
                    .SetEnvironment(WalletConstants.EnvironmentTest)
#else
                    .SetEnvironment(WalletConstants.EnvironmentProduction)
#endif
                    .Build());

            OnIsSupportedRequestReceived += async tcs =>
            {

                try
                {
                    var jsonString = JsonConvert.SerializeObject(new
                    {
                        apiversion = 2,
                        apiVersionMinor = 0,
                        allowedPaymentMethods = new[] { new
                            {
                                type = "CARD",
                                parameters = new
                                {
                                    allowedAuthMethods = new[] { "PAN_ONLY", "CRYPTOGRAM_3DS" },
                                    allowedCardNetworks = new[] { "AMEX", "MASTERCARD", "VISA" }
                                }
                            }
                        }
                    });
                    tcs.SetResult((await paymentsClient
                        .IsReadyToPayAsync(IsReadyToPayRequest.FromJson(jsonString)))
                        .BooleanValue());
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            OnGetTokenRequestReceived += (tcs, totalToPay) =>
            {

                try
                {
                    var pMethods = new List<Java.Lang.Integer> { 
                        (Java.Lang.Integer)WalletConstants.CardNetworkAmex,
                        (Java.Lang.Integer)WalletConstants.CardNetworkVisa,
                        (Java.Lang.Integer)WalletConstants.CardNetworkMastercard };
                    var req = PaymentDataRequest.NewBuilder()
                    .SetTransactionInfo(
                        TransactionInfo.NewBuilder()
                            .SetTotalPriceStatus(WalletConstants.TotalPriceStatusFinal)
                            .SetTotalPrice((totalToPay / 100d).ToString("0.00"))
                            .SetCurrencyCode("GBP")
                            .Build())
                    .AddAllowedPaymentMethod(WalletConstants.CardClassCredit)
                    .AddAllowedPaymentMethod(WalletConstants.PaymentMethodTokenizedCard)
                    .SetCardRequirements(
                        CardRequirements.NewBuilder()
                        .AddAllowedCardNetworks(pMethods)
                            .Build())
                    .SetPaymentMethodTokenizationParameters(PaymentMethodTokenizationParameters.NewBuilder()
                            .SetPaymentMethodTokenizationType(
                                WalletConstants.PaymentMethodTokenizationTypePaymentGateway)
                            .AddParameter("gateway", "stripe")
                            .AddParameter("stripe:publishableKey", App.StripeKey)
                            .AddParameter("stripe:version", "2020-08-27")
                            .Build())
                    .Build();
                    AutoResolveHelper.ResolveTask(paymentsClient.LoadPaymentData(req), activity, PaymentDataRequestCode);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            };

            isInitialised = true;
        }
    }
}