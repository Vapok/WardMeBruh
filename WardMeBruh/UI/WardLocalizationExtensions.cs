using System;

namespace WardMeBruh.UI
{
    public static class WardLocalizationExtensions
    {
        public static string Localize(this string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (Localization.instance != null)
            {
                return Localization.instance.Localize(text);
            }

            return text;
        }

        public static string Localize(this string text, params string[] words)
        {
            if (string.IsNullOrEmpty(text))
            {
                return string.Empty;
            }

            if (Localization.instance != null)
            {
                return Localization.instance.Localize(text, words);
            }

            return text;
        }
    }
}
