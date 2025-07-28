using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
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
        private DispatcherTimer _saveSuccessTimer;

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
            
            // 初始化保存成功定时器
            _saveSuccessTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _saveSuccessTimer.Tick += ResetSaveButtonStyle;
            
            // 设置初始状态
            UpdateStatus("就绪");
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
                UpdateStatus($"正在加载文件夹: {Path.GetFileName(_currentPath)}");
                LoadFolders();
                _folderWatcher.StartWatching(_currentPath);
                UpdateStatus($"已加载 {_folders.Count} 个文件夹");
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
                UpdateStatus("加载文件夹失败");
            }
        }

        private void FolderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FolderListBox.SelectedItem is FolderInfo selectedFolder)
            {
                _selectedFolder = selectedFolder;
                SelectedFolderLabel.Text = $"📁 {selectedFolder.Name}";
                FolderPathLabel.Text = selectedFolder.FullPath;
                RemarkTextBox.Text = selectedFolder.Remark == "Default" ? "" : selectedFolder.Remark;
                SaveButton.IsEnabled = true;
                UpdateStatus($"已选择文件夹: {selectedFolder.Name}");
            }
            else
            {
                _selectedFolder = null;
                SelectedFolderLabel.Text = "💡 请选择一个文件夹";
                FolderPathLabel.Text = "";
                RemarkTextBox.Text = "";
                SaveButton.IsEnabled = false;
                UpdateStatus("请选择一个文件夹");
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFolder == null)
                return;

            try
            {
                var remark = string.IsNullOrWhiteSpace(RemarkTextBox.Text) ? "Default" : RemarkTextBox.Text.Trim();
                
                _selectedFolder.Remark = remark;
                _remarkService.SetRemark(_selectedFolder.FullPath, remark);
                _remarkService.SaveRemarks();
                
                // 显示保存成功的视觉反馈
                ShowSaveSuccess();
                UpdateStatus("备注保存成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存备注时出错: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus("保存失败");
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentPath))
            {
                UpdateStatus("正在刷新...");
                LoadFolders();
                UpdateStatus($"刷新完成，共 {_folders.Count} 个文件夹");
            }
            else
            {
                UpdateStatus("请先选择一个文件夹路径");
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
                UpdateStatus($"检测到新文件夹: {folderInfo.Name}");

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
                    UpdateStatus($"文件夹已删除: {Path.GetFileName(folderPath)}");
                    
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
            _saveSuccessTimer?.Stop();
        }

        private void ShowSaveSuccess()
        {
            // 停止之前的定时器
            _saveSuccessTimer.Stop();
            
            // 更改按钮样式为成功状态
            SaveButton.Style = (Style)FindResource("SaveSuccessButtonStyle");
            SaveButton.Content = "✅ 保存成功";
            
            // 启动定时器，2秒后恢复原样
            _saveSuccessTimer.Start();
        }

        private void ResetSaveButtonStyle(object sender, EventArgs e)
        {
            // 恢复按钮原始样式和内容
            SaveButton.Style = (Style)FindResource("ModernButtonStyle");
            SaveButton.Content = "💾 保存备注";
            
            // 停止定时器
            _saveSuccessTimer.Stop();
        }

        private void UpdateStatus(string message)
        {
            StatusLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }
    }
}