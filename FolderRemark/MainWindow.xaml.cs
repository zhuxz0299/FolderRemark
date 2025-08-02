using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FolderRemark.Models;
using FolderRemark.Services;
using FolderRemark.Windows;
using MessageBox = System.Windows.MessageBox;
using WinApplication = System.Windows.Application;

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
        private readonly SettingsService _settingsService;
        private FolderInfo _selectedFolder;
        private string _currentPath;
        private DispatcherTimer _saveSuccessTimer;
        private NotifyIcon _notifyIcon;

        public MainWindow()
        {
            // 初始化设置服务
            _settingsService = new SettingsService();
            
            // 应用保存的主题和字体设置
            ApplySettings();
            
            InitializeComponent();
            
            // 设置窗口图标
            SetWindowIcon();
            
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
            
            // 初始化系统托盘
            InitializeNotifyIcon();
            
            // 设置初始状态
            UpdateStatus("就绪");
            
            // 如果有保存的路径，自动加载
            LoadLastSelectedPath();
        }

        private void SetWindowIcon()
        {
            var iconSource = IconHelper.CreateWindowIcon();
            if (iconSource != null)
            {
                this.Icon = iconSource;
            }
        }

        private void InitializeNotifyIcon()
        {
            _notifyIcon = new NotifyIcon();
            
            // 设置托盘图标 - 使用IconHelper创建
            _notifyIcon.Icon = IconHelper.CreateTrayIcon();
            _notifyIcon.Text = "文件夹备注工具";
            _notifyIcon.Visible = false; // 初始时不显示，只有在最小化到托盘时才显示
            
            // 双击托盘图标恢复窗口
            _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            
            // 创建右键菜单
            var contextMenu = new ContextMenuStrip();
            
            var showItem = new ToolStripMenuItem("显示主窗口");
            showItem.Click += (s, e) => ShowMainWindow();
            contextMenu.Items.Add(showItem);
            
            contextMenu.Items.Add(new ToolStripSeparator());
            
            var exitItem = new ToolStripMenuItem("退出程序");
            exitItem.Click += (s, e) => ExitApplication();
            contextMenu.Items.Add(exitItem);
            
            _notifyIcon.ContextMenuStrip = contextMenu;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            _notifyIcon.Visible = false;
        }

        private void ExitApplication()
        {
            _notifyIcon.Visible = false;
            _notifyIcon?.Dispose();
            WinApplication.Current.Shutdown();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized && _settingsService.Settings.MinimizeToTray)
            {
                Hide();
                _notifyIcon.Visible = true;
                _notifyIcon.ShowBalloonTip(2000, "文件夹备注工具", "程序已最小化到系统托盘", ToolTipIcon.Info);
            }
            base.OnStateChanged(e);
        }

        private void ApplySettings()
        {
            var settings = _settingsService?.Settings;
            if (settings != null)
            {
                // 应用主题
                ThemeManager.ApplyTheme(settings.Theme);
                
                // 设置字体大小
                WinApplication.Current.Resources["AppFontSize"] = settings.FontSize;
            }
        }

        private void LoadLastSelectedPath()
        {
            var lastPath = _settingsService.Settings.LastSelectedPath;
            if (!string.IsNullOrEmpty(lastPath) && Directory.Exists(lastPath))
            {
                _currentPath = lastPath;
                PathTextBox.Text = _currentPath;
                LoadFolders();
                _folderWatcher.StartWatching(_currentPath);
                UpdateStatus($"已自动加载上次路径: {Path.GetFileName(_currentPath)}");
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择要监控的文件夹",
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true,
                SelectedPath = _currentPath
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _currentPath = dialog.SelectedPath;
                PathTextBox.Text = _currentPath;
                
                // 保存选择的路径
                _settingsService.UpdateLastSelectedPath(_currentPath);
                
                UpdateStatus($"正在加载文件夹: {Path.GetFileName(_currentPath)}");
                LoadFolders();
                _folderWatcher.StartWatching(_currentPath);
                UpdateStatus($"已加载 {_folders.Count} 个文件夹");
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new Windows.SettingsWindow(_settingsService)
            {
                Owner = this
            };

            if (settingsWindow.ShowDialog() == true)
            {
                // 重新应用设置
                ApplySettings();
                UpdateStatus("设置已更新");
                
                // 如果路径改变了，重新加载
                var newPath = _settingsService.Settings.LastSelectedPath;
                if (!string.IsNullOrEmpty(newPath) && newPath != _currentPath && Directory.Exists(newPath))
                {
                    _currentPath = newPath;
                    PathTextBox.Text = _currentPath;
                    LoadFolders();
                    _folderWatcher.StartWatching(_currentPath);
                    UpdateStatus($"已切换到新路径: {Path.GetFileName(_currentPath)}");
                }
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
            // 如果启用了托盘模式，点击关闭按钮时最小化到托盘而不是退出
            if (_settingsService.Settings.MinimizeToTray)
            {
                e.Cancel = true;
                WindowState = WindowState.Minimized;
                return;
            }
            
            // 正常关闭流程
            _remarkService.SaveRemarks();
            _settingsService.SaveSettings();
            _folderWatcher?.Dispose();
            _saveSuccessTimer?.Stop();
            _notifyIcon?.Dispose();
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

        private void UpdateStatus(String message)
        {
            StatusLabel.Text = $"{DateTime.Now:HH:mm:ss} - {message}";
        }
    }
}