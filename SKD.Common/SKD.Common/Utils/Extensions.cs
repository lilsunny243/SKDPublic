using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using SDColour = System.Drawing.Color;

namespace SKD.Common.Utils
{
    public static class Extensions
    {
        public static string PascalToSnakeCase(this string s)
        {
            StringBuilder sb = new StringBuilder(s.ToLower()[0].ToString());
            char prev = s[0];
            foreach (char c in s.Substring(1))
            {
                if (char.IsUpper(c) && char.IsLower(prev))
                    sb.Append("_");
                sb.Append(c.ToString().ToLower());
                prev = c;
            }
            return sb.ToString();
        }

        public static bool IsNumeric(this string s)
        {
            foreach (char c in s)
                if (!char.IsDigit(c))
                    return false;
            return true;
        }

        public static SDColour ToSDColour(this Color colour) =>
            SDColour.FromArgb((int)(colour.A * 255), (int)(colour.R * 255), (int)(colour.G * 255), (int)(colour.B * 255));

        public static string ToHexRGBA(this Color colour) => "#" + colour.ToHex().Substring(3, 6) + colour.ToHex().Substring(1, 2);

        public static int Product(this IEnumerable<int> ints) => ints.Aggregate(1, (acc, val) => acc * val);
        public static double Product(this IEnumerable<double> doubles) => doubles.Aggregate(1d, (acc, val) => acc * val);

        public static string GetCoordString(this Point p) => FormattableString.Invariant($"{p.X},{p.Y}");

        public static Uri ToUri(this string s) => new Uri(s);

        public static void Iteri<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            for (int i = 0; i < enumerable.Count(); i++)
                action(enumerable.ElementAt(i), i);
        }
    }
}
