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

            app.Resources["BgPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F0F2F5"));
            app.Resources["BgSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            app.Resources["BgMenu"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
            app.Resources["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C3E50"));
            app.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F8C8D"));
            app.Resources["TextOnDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            app.Resources["Accent"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3498DB"));
            app.Resources["Success"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#27AE60"));
            app.Resources["Danger"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            app.Resources["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E0E0E0"));
            app.Resources["CardBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
            app.Resources["InputBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFFFFF"));
        }

        public static void SetDarkTheme()
        {
            IsDark = true;
            var app = Application.Current;

            app.Resources["BgPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A1A2E"));
            app.Resources["BgSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#16213E"));
            app.Resources["BgMenu"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0F0F1A"));
            app.Resources["TextPrimary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAEAEA"));
            app.Resources["TextSecondary"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A0A0A0"));
            app.Resources["TextOnDark"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EAEAEA"));
            app.Resources["Accent"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4EA8DE"));
            app.Resources["Success"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2ECC71"));
            app.Resources["Danger"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E74C3C"));
            app.Resources["BorderColor"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A2A4A"));
            app.Resources["CardBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F1F3A"));
            app.Resources["InputBg"] = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1F1F3A"));
        }

        public static void ToggleTheme()
        {
            if (IsDark)
                SetLightTheme();
            else
                SetDarkTheme();
        }
    }
}