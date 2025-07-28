using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using FolderRemark.Models;
using FolderRemark.Services;
using MessageBox = System.Windows.MessageBox;

namespace FolderRemark
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<FolderInfo> _folders;
        private readonly RemarkService _remarkService;
        private readonly FolderWatcher _folderWatcher;
        private FolderInfo _selectedFolder;
        private string _currentPath;

        public MainWindow()
        {
            InitializeComponent();
            
            _folders = new ObservableCollection<FolderInfo>();
            _remarkService = new RemarkService();
            _folderWatcher = new FolderWatcher();
            
            FolderListBox.ItemsSource = _folders;
            
            // 订阅文件夹变化事件
            _folderWatcher.FolderAdded += OnFolderAdded;
            _folderWatcher.FolderDeleted += OnFolderDeleted;
            
            // 窗口关闭时保存数据
            Closing += MainWindow_Closing;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择要监控的文件夹",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _currentPath = dialog.SelectedPath;
                PathTextBox.Text = _currentPath;
                LoadFolders();
                _folderWatcher.StartWatching(_currentPath);
            }
        }

        private void LoadFolders()
        {
            _folders.Clear();
            
            if (string.IsNullOrEmpty(_currentPath) || !Directory.Exists(_currentPath))
                return;

            try
            {
                var directories = Directory.GetDirectories(_currentPath);
                foreach (var dir in directories)
                {
                    var folderInfo = new FolderInfo
                    {
                        Name = Path.GetFileName(dir),
                        FullPath = dir,
                        Remark = _remarkService.GetRemark(dir)
                    };
                    _folders.Add(folderInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载文件夹时出错: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void FolderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FolderListBox.SelectedItem is FolderInfo selectedFolder)
            {
                _selectedFolder = selectedFolder;
                SelectedFolderLabel.Text = $"文件夹: {selectedFolder.Name}";
                FolderPathLabel.Text = selectedFolder.FullPath;
                RemarkTextBox.Text = selectedFolder.Remark == "Default" ? "" : selectedFolder.Remark;
                SaveButton.IsEnabled = true;
            }
            else
            {
                _selectedFolder = null;
                SelectedFolderLabel.Text = "请选择一个文件夹";
                FolderPathLabel.Text = "";
                RemarkTextBox.Text = "";
                SaveButton.IsEnabled = false;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFolder == null)
                return;

            var remark = string.IsNullOrWhiteSpace(RemarkTextBox.Text) ? "Default" : RemarkTextBox.Text.Trim();
            
            _selectedFolder.Remark = remark;
            _remarkService.SetRemark(_selectedFolder.FullPath, remark);
            _remarkService.SaveRemarks();
            
            MessageBox.Show("备注已保存！", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentPath))
            {
                LoadFolders();
            }
        }

        private void OnFolderAdded(string folderPath)
        {
            Dispatcher.Invoke(() =>
            {
                // 检查文件夹是否已存在于列表中
                if (_folders.Any(f => f.FullPath == folderPath))
                    return;

                var folderInfo = new FolderInfo
                {
                    Name = Path.GetFileName(folderPath),
                    FullPath = folderPath,
                    Remark = _remarkService.GetRemark(folderPath)
                };
                
                _folders.Add(folderInfo);

                // 弹窗提醒用户添加备注
                var result = MessageBox.Show(
                    $"检测到新文件夹: {folderInfo.Name}\n是否要为其添加备注？", 
                    "新文件夹", 
                    MessageBoxButton.YesNo, 
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    FolderListBox.SelectedItem = folderInfo;
                    RemarkTextBox.Focus();
                }
            });
        }

        private void OnFolderDeleted(string folderPath)
        {
            Dispatcher.Invoke(() =>
            {
                var folderToRemove = _folders.FirstOrDefault(f => f.FullPath == folderPath);
                if (folderToRemove != null)
                {
                    _folders.Remove(folderToRemove);
                    
                    // 如果删除的是当前选中的文件夹，清空选择
                    if (_selectedFolder?.FullPath == folderPath)
                    {
                        FolderListBox.SelectedItem = null;
                    }
                }
            });
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _remarkService.SaveRemarks();
            _folderWatcher?.Dispose();
        }
    }
}