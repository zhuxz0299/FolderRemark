using System;
using System.IO;
using System.Text.Json;
using FolderRemark.Models;

namespace FolderRemark.Services
{
    public class SettingsService
    {
        private readonly string _settingsFilePath;
        private AppSettings _settings;

        public SettingsService()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var appFolder = Path.Combine(appDataPath, "FolderRemark");
            Directory.CreateDirectory(appFolder);
            _settingsFilePath = Path.Combine(appFolder, "settings.json");
            LoadSettings();
        }

        public AppSettings Settings => _settings;

        public void SaveSettings()
        {
            try
            {
                var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存设置时出错: {ex.Message}", "错误",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
                else
                {
                    _settings = new AppSettings();
                }
            }
            catch (Exception)
            {
                _settings = new AppSettings();
            }
        }

        public void UpdateFontSize(double fontSize)
        {
            _settings.FontSize = fontSize;
            SaveSettings();
        }

        public void UpdateTheme(string theme)
        {
            _settings.Theme = theme;
            SaveSettings();
        }

        public void UpdateLastSelectedPath(string path)
        {
            _settings.LastSelectedPath = path;
            SaveSettings();
        }
    }
}