using CreditCardValidator;
using DonorClient.Services;
using Plugin.FirebaseAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DonorClient.Utils
{
    public static class Extensions
    {
        public static bool IsAccepted(this CardIssuer issuer) => acceptedCardIssuers.Contains(issuer);

        public static bool IsAmex(this string cardNumber) => !string.IsNullOrWhiteSpace(cardNumber) && cardNumber.CreditCardBrandIgnoreLength() == CardIssuer.AmericanExpress;

        private static readonly CardIssuer[] acceptedCardIssuers = new[] { CardIssuer.Visa, CardIssuer.MasterCard, CardIssuer.AmericanExpress };

        public static string InsertPascalSpaces(this string s)
        {
            StringBuilder sb = new StringBuilder(s[0].ToString());
            for(int i = 1; i < s.Length; i++)
            {
                if (char.IsUpper(s[i]) && char.IsLower(s[i-1]))
                    sb.Append(" ");
                sb.Append(s[i]);
            }
            return sb.ToString();
        }

        public static string GetFirebaseId(this AuthProvider provider) => provider.ToString().ToLower() + ".com";

        public static OAuthProvider GetOAuthProvider(this AuthProvider provider) => provider switch
        {
            AuthProvider.Apple => new OAuthProvider("apple.com"),
            AuthProvider.GitHub => new OAuthProvider("github.com") { Scopes = new[] { "read:user", "user:email" } },
            AuthProvider.Microsoft => new OAuthProvider("microsoft.com")
            {
                Scopes = new[] { "openid", "email", "profile" },
                CustomParameters = new Dictionary<string, string> { ["tenant"] = "common" }
            },
            AuthProvider.Twitter => new OAuthProvider("twitter.com"),
            _ => throw new InvalidOperationException("Cannot get OAuthProvider for AuthProvider: " + provider.ToString())
        };
    }
}
