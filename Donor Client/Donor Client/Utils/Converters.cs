using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace DonorClient.Utils
{
    public class StringToString_AutoDashConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
                return text;
            var mask = text!.IsAmex() ? "xxxx-xxxxxx-xxxxx" : "xxxx-xxxx-xxxx-xxxx";
            int bump = 0;
            var builder = new StringBuilder();
            for (int i = 0; i < text!.Length; i++)
            {
                if (mask[i + bump] == '-')
                {
                    builder.Append('-');
                    bump++;
                }
                builder.Append(text[i]);
            }
            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string)!.Replace("-", string.Empty);
        }
    }

    public class MonthYearConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (string.IsNullOrWhiteSpace(text))
                return text;
            var builder = new StringBuilder();
            for (int i = 0; i < text!.Length; i++)
            {
                builder.Append(text[i]);
                if (i == 1)
                    builder.Append('/');
            }
            return builder.ToString();
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (text?.Contains("/") ?? false)
                return text.Replace('/', ',');
            return text;
        }
    }
}
