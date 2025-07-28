using System;
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
            InitializeControls();
        }

        private void InitializeControls()
        {
            // ���������С����
            FontSizeSlider.Value = _settings.FontSize;

            // �������ⵥѡ��ť
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

            // ����Ĭ��·��
            DefaultPathTextBox.Text = _settings.LastSelectedPath;
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
                // ����Ӧ������Ԥ��
                ThemeManager.ApplyTheme(theme);
            }
        }

        private void BrowsePathButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "ѡ��Ĭ�ϼ���ļ���",
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
                // ����·������
                _settings.LastSelectedPath = DefaultPathTextBox.Text;
                
                // ������������
                _settingsService.SaveSettings();
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"��������ʱ����: {ex.Message}", "����",
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
                "ȷ��Ҫ������������ΪĬ��ֵ��", 
                "ȷ������", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                // ����ΪĬ��ֵ
                _settings.FontSize = 13;
                _settings.Theme = "Light";
                _settings.LastSelectedPath = string.Empty;
                
                // ���½���
                InitializeControls();
                
                // Ӧ��Ĭ������
                ThemeManager.ApplyTheme("Light");
            }
        }
    }
}