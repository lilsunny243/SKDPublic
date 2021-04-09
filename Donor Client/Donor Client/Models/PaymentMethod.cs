using CreditCardValidator;
using DonorClient.Utils;
using Plugin.CloudFirestore.Attributes;
using Plugin.CloudFirestore.Converters;
using SKD.Common.Models;
using SKD.Common.Themes;
using SKD.Common.Utils;
using System;
using Xamarin.Forms;

namespace DonorClient.Models
{
    public class PaymentMethod : BaseModel
    {
        public int ExpiryMonth { get; set; }
        public int ExpiryYear { get; set; }

        [Ignored]
        public bool IsExpired => ExpiryYear < DateTime.Today.Year || (ExpiryYear == DateTime.Today.Year && ExpiryMonth <= DateTime.Today.Month);

        public string LastFourDigits { get; set; } = "xxxx";

        [DocumentConverter(typeof(EnumStringConverter))]
        public CardIssuer Provider { get; set; }

        public string? StripeId { get; set; }
        public string? StripeToken { get; set; }

        [DocumentConverter(typeof(StripeStatusConverter))]
        public StripeStatus StripeStatus { get; set; }

        public string? StripeRedirectUrl { get; set; }

        public static ImageSource GetIconImageSource(CardIssuer issuer) => ImageSource.FromFile(
            issuer.IsAccepted() ? issuer.ToString().PascalToSnakeCase() : (ThemeEngine.IsEffectivelyLight ? "unknown_light" : "unknown_dark") + ".png");

        public static ImageSource GetIconImageSource(string provider) => ImageSource.FromFile(provider.PascalToSnakeCase() + ".png");
    }


}
