using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;

namespace FolderRemark.Services
{
    public static class IconHelper
    {
        /// <summary>
        /// 创建托盘图标
        /// </summary>
        /// <returns></returns>
        public static Icon CreateTrayIcon()
        {
            // 创建一个更好看的16x16像素的图标
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // 绘制一个圆形背景
                g.FillEllipse(System.Drawing.Brushes.DodgerBlue, 1, 1, 14, 14);
                
                // 绘制文件夹图标轮廓
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.White, 1))
                {
                    // 文件夹形状
                    g.DrawRectangle(pen, 3, 6, 10, 6);
                    g.DrawLine(pen, 3, 6, 6, 6);
                    g.DrawLine(pen, 6, 6, 7, 4);
                    g.DrawLine(pen, 7, 4, 10, 4);
                    g.DrawLine(pen, 10, 4, 10, 6);
                }
                
                // 填充文件夹
                using (var brush = new SolidBrush(System.Drawing.Color.FromArgb(200, System.Drawing.Color.White)))
                {
                    g.FillRectangle(brush, 4, 7, 8, 4);
                }
            }
            var hIcon = bitmap.GetHicon();
            return System.Drawing.Icon.FromHandle(hIcon);
        }

        /// <summary>
        /// 创建窗口图标 (32x32)
        /// </summary>
        /// <returns></returns>
        public static BitmapSource CreateWindowIcon()
        {
            try
            {
                // 创建一个32x32像素的图标，用于窗口标题栏
                var bitmap = new Bitmap(32, 32);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    
                    // 绘制一个圆形背景
                    g.FillEllipse(System.Drawing.Brushes.DodgerBlue, 2, 2, 28, 28);
                    
                    // 绘制文件夹图标轮廓 - 放大版本
                    using (var pen = new System.Drawing.Pen(System.Drawing.Color.White, 2))
                    {
                        // 文件夹形状 - 按比例放大
                        g.DrawRectangle(pen, 6, 12, 20, 12);
                        g.DrawLine(pen, 6, 12, 12, 12);
                        g.DrawLine(pen, 12, 12, 14, 8);
                        g.DrawLine(pen, 14, 8, 20, 8);
                        g.DrawLine(pen, 20, 8, 20, 12);
                    }
                    
                    // 填充文件夹
                    using (var brush = new SolidBrush(System.Drawing.Color.FromArgb(200, System.Drawing.Color.White)))
                    {
                        g.FillRectangle(brush, 8, 14, 16, 8);
                    }
                }
                
                var hIcon = bitmap.GetHicon();
                
                // 将System.Drawing.Icon转换为WPF ImageSource
                var iconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                
                bitmap.Dispose();
                
                return iconSource;
            }
            catch (Exception ex)
            {
                // 如果图标创建失败，返回null
                System.Diagnostics.Debug.WriteLine($"创建窗口图标时出错: {ex.Message}");
                return null;
            }
        }
    }
}