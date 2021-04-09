using System;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Converters;

namespace SKD.Common.Models
{
    public enum StripeStatus
    {
        Pending,
        Processing,
        RequiresAction,
        Succeeded,
        Failed
    }

    public static class StripeStatusExtensions
    {
        public static string? GetStringValue(this StripeStatus status) => status switch
        {
            StripeStatus.Pending => "pending",
            StripeStatus.Processing => "processing",
            StripeStatus.RequiresAction => "requires_action",
            StripeStatus.Succeeded => "succeeded",
            StripeStatus.Failed => "requires_payment_method",
            _ => null
        };
    }

    public class StripeStatusConverter : DocumentConverter
    {
        public StripeStatusConverter(Type targetType) : base(targetType)
        {
            if (targetType != typeof(StripeStatus))
                throw new ArgumentException("Target Type must be StripeStatus", nameof(targetType));
        }

        public override bool ConvertFrom(DocumentObject value, out object? result)
        {
            result = value.String switch
            {
                "pending" => StripeStatus.Pending,
                "processing" => StripeStatus.Processing,
                "requires_action" => StripeStatus.RequiresAction,
                "succeeded" => StripeStatus.Succeeded,
                "requires_payment_method" => StripeStatus.Failed,
                _ => null
            };
            return !(result is null);
        }

        public override bool ConvertTo(object? value, out object? result)
        {
            result = (value as StripeStatus?)?.GetStringValue();
            return !(result is null);
        }
    }
}
