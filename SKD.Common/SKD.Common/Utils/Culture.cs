using System.Globalization;

namespace SKD.Common.Utils
{
    public static class Culture
    {
        public static bool IsSpanish => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "es";
    }
}
