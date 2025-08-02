using System;
using System.IO;
using System.Reflection;
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
            // 获取可执行文件目录
            var exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) 
                              ?? AppDomain.CurrentDomain.BaseDirectory;
            
            // 在可执行文件目录下创建Data文件夹用于存放数据文件
            var dataFolder = Path.Combine(exeDirectory, "Data");
            Directory.CreateDirectory(dataFolder);
            
            _settingsFilePath = Path.Combine(dataFolder, "settings.json");
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

        public void UpdateMinimizeToTray(bool minimizeToTray)
        {
            _settings.MinimizeToTray = minimizeToTray;
            SaveSettings();
        }
    }
}