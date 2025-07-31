using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using FolderRemark.Models;

namespace FolderRemark.Services
{
    public class RemarkService
    {
        private readonly string _dataFilePath;
        private Dictionary<string, string> _remarks;

        public RemarkService()
        {
            // 获取可执行文件目录
            var exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) 
                              ?? AppDomain.CurrentDomain.BaseDirectory;
            
            // 在可执行文件目录下创建Data文件夹用于存放数据文件
            var dataFolder = Path.Combine(exeDirectory, "Data");
            Directory.CreateDirectory(dataFolder);
            
            _dataFilePath = Path.Combine(dataFolder, "remarks.json");
            LoadRemarks();
        }

        public string GetRemark(string folderPath)
        {
            return _remarks.TryGetValue(folderPath, out var remark) ? remark : "Default";
        }

        public void SetRemark(string folderPath, string remark)
        {
            _remarks[folderPath] = remark ?? "Default";
        }

        public void SaveRemarks()
        {
            try
            {
                var json = JsonSerializer.Serialize(_remarks, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_dataFilePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"保存备注时出错: {ex.Message}", "错误", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void LoadRemarks()
        {
            try
            {
                if (File.Exists(_dataFilePath))
                {
                    var json = File.ReadAllText(_dataFilePath);
                    _remarks = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new Dictionary<string, string>();
                }
                else
                {
                    _remarks = new Dictionary<string, string>();
                }
            }
            catch (Exception)
            {
                _remarks = new Dictionary<string, string>();
            }
        }
    }
}