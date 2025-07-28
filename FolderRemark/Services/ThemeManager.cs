using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace FolderRemark.Services
{
    public class ThemeManager
    {
        public static void ApplyTheme(string themeName)
        {
            var app = System.Windows.Application.Current;
            if (app?.Resources == null) return;

            // 清除之前的主题资源
            app.Resources.MergedDictionaries.Clear();

            // 根据主题名称应用不同的颜色方案
            switch (themeName)
            {
                case "Light":
                    ApplyLightTheme(app);
                    break;
                case "Dark":
                    ApplyDarkTheme(app);
                    break;
                case "System":
                    ApplySystemTheme(app);
                    break;
                default:
                    ApplyLightTheme(app);
                    break;
            }
        }

        private static void ApplyLightTheme(System.Windows.Application app)
        {
            app.Resources["WindowBackgroundBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(248, 249, 250));
            app.Resources["CardBackgroundBrush"] = new SolidColorBrush(Colors.White);
            app.Resources["PrimaryBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 204));
            app.Resources["PrimaryHoverBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 90, 158));
            app.Resources["SuccessBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 167, 69));
            app.Resources["SecondaryBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 117, 125));
            app.Resources["TextBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(73, 80, 87));
            app.Resources["BorderBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(222, 226, 230));
            app.Resources["StatusBarBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(73, 80, 87));
        }

        private static void ApplyDarkTheme(System.Windows.Application app)
        {
            app.Resources["WindowBackgroundBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(32, 32, 32));
            app.Resources["CardBackgroundBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(48, 48, 48));
            app.Resources["PrimaryBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 122, 204));
            app.Resources["PrimaryHoverBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 90, 158));
            app.Resources["SuccessBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 167, 69));
            app.Resources["SecondaryBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(108, 117, 125));
            app.Resources["TextBrush"] = new SolidColorBrush(Colors.White);
            app.Resources["BorderBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(64, 64, 64));
            app.Resources["StatusBarBrush"] = new SolidColorBrush(System.Windows.Media.Color.FromRgb(24, 24, 24));
        }

        private static void ApplySystemTheme(System.Windows.Application app)
        {
            // 检测系统主题
            if (IsSystemDarkTheme())
            {
                ApplyDarkTheme(app);
            }
            else
            {
                ApplyLightTheme(app);
            }
        }

        private static bool IsSystemDarkTheme()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
                var value = key?.GetValue("AppsUseLightTheme");
                return value is int intValue && intValue == 0;
            }
            catch
            {
                return false; // 默认为浅色主题
            }
        }
    }
}