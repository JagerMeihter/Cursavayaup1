using System.Windows;
using System.Windows.Media;

namespace Cursavayaup1.Helpers
{
    public static class ThemeManager
    {
        public static bool IsDark { get; private set; } = false;

        public static void SetLightTheme()
        {
            IsDark = false;
            var app = Application.Current;

            // Фон
            app.Resources["BgPrimary"] = Brush("#F3F3F3");   // Pantone 649 C
            app.Resources["BgSecondary"] = Brush("#FFFFFF");   // White
            app.Resources["BgMenu"] = Brush("#233E76");   // Pantone 7687 C
            app.Resources["CardBg"] = Brush("#FFFFFF");
            app.Resources["InputBg"] = Brush("#FFFFFF");

            // Текст
            app.Resources["TextPrimary"] = Brush("#233E76");
            app.Resources["TextSecondary"] = Brush("#917AA3");
            app.Resources["TextOnDark"] = Brush("#FFFFFF");

            // Акценты
            app.Resources["Accent"] = Brush("#0198AF");      // Pantone 7711 C
            app.Resources["Success"] = Brush("#4DC6C5");      // светлая бирюза
            app.Resources["Danger"] = Brush("#DD4B42");      // Pantone 7625 C
            app.Resources["BorderColor"] = Brush("#EAEBED");  // Pantone 663 C
        }

        public static void SetDarkTheme()
        {
            IsDark = true;
            var app = Application.Current;

            // Фон
            app.Resources["BgPrimary"] = Brush("#193D57");  // Pantone 534 C
            app.Resources["BgSecondary"] = Brush("#1A2A3A");
            app.Resources["BgMenu"] = Brush("#0F1C2A");
            app.Resources["CardBg"] = Brush("#243447");
            app.Resources["InputBg"] = Brush("#243447");

            // Текст
            app.Resources["TextPrimary"] = Brush("#EAEBED");
            app.Resources["TextSecondary"] = Brush("#917AA3");
            app.Resources["TextOnDark"] = Brush("#EAEBED");

            // Акценты
            app.Resources["Accent"] = Brush("#4DC6C5");
            app.Resources["Success"] = Brush("#4DC6C5");
            app.Resources["Danger"] = Brush("#DD4B42");
            app.Resources["BorderColor"] = Brush("#2A3F55");
        }

        public static void ToggleTheme()
        {
            if (IsDark)
                SetLightTheme();
            else
                SetDarkTheme();
        }

        private static SolidColorBrush Brush(string hex)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
    }
}