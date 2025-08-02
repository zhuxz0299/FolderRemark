using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using FolderRemark.Models;
using FolderRemark.Services;

namespace FolderRemark.Windows
{
    public partial class SettingsWindow : Window
    {
        private readonly SettingsService _settingsService;
        private readonly AppSettings _settings;

        public SettingsWindow(SettingsService settingsService)
        {
            InitializeComponent();
            _settingsService = settingsService;
            _settings = settingsService.Settings;
            
            DataContext = _settings;
            
            // 设置窗口图标
            SetWindowIcon();
            
            InitializeControls();
        }

        private void SetWindowIcon()
        {
            var iconSource = IconHelper.CreateWindowIcon();
            if (iconSource != null)
            {
                this.Icon = iconSource;
            }
        }

        private void InitializeControls()
        {
            // 设置字体大小滑块
            FontSizeSlider.Value = _settings.FontSize;

            // 设置主题单选按钮
            switch (_settings.Theme)
            {
                case "Light":
                    LightThemeRadio.IsChecked = true;
                    break;
                case "Dark":
                    DarkThemeRadio.IsChecked = true;
                    break;
                case "System":
                    SystemThemeRadio.IsChecked = true;
                    break;
            }

            // 设置默认路径
            DefaultPathTextBox.Text = _settings.LastSelectedPath;

            // 设置托盘选项
            MinimizeToTrayCheckBox.IsChecked = _settings.MinimizeToTray;
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_settings != null)
            {
                _settings.FontSize = e.NewValue;
            }
        }

        private void ThemeRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.RadioButton radio && radio.Tag is string theme)
            {
                _settings.Theme = theme;
                // 立即应用主题预览
                ThemeManager.ApplyTheme(theme);
            }
        }

        private void MinimizeToTrayCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.MinimizeToTray = true;
            }
        }

        private void MinimizeToTrayCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (_settings != null)
            {
                _settings.MinimizeToTray = false;
            }
        }

        private void BrowsePathButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择默认监控文件夹",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                SelectedPath = _settings.LastSelectedPath
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                DefaultPathTextBox.Text = dialog.SelectedPath;
                _settings.LastSelectedPath = dialog.SelectedPath;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 保存路径设置
                _settings.LastSelectedPath = DefaultPathTextBox.Text;
                
                // 保存所有设置
                _settingsService.SaveSettings();
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存设置时出错: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show(
                "确定要重置所有设置为默认值吗？", 
                "确认重置", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // 重置为默认值
                _settings.FontSize = 13;
                _settings.Theme = "Light";
                _settings.LastSelectedPath = string.Empty;
                _settings.MinimizeToTray = false;
                
                // 更新界面
                InitializeControls();
                
                // 应用默认主题
                ThemeManager.ApplyTheme("Light");
            }
        }
    }
}