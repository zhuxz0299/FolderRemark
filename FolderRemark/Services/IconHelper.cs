using System;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows;

namespace FolderRemark.Services
{
    public static class IconHelper
    {
        /// <summary>
        /// ��������ͼ��
        /// </summary>
        /// <returns></returns>
        public static Icon CreateTrayIcon()
        {
            // ����һ�����ÿ���16x16���ص�ͼ��
            var bitmap = new Bitmap(16, 16);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(System.Drawing.Color.Transparent);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                
                // ����һ��Բ�α���
                g.FillEllipse(System.Drawing.Brushes.DodgerBlue, 1, 1, 14, 14);
                
                // �����ļ���ͼ������
                using (var pen = new System.Drawing.Pen(System.Drawing.Color.White, 1))
                {
                    // �ļ�����״
                    g.DrawRectangle(pen, 3, 6, 10, 6);
                    g.DrawLine(pen, 3, 6, 6, 6);
                    g.DrawLine(pen, 6, 6, 7, 4);
                    g.DrawLine(pen, 7, 4, 10, 4);
                    g.DrawLine(pen, 10, 4, 10, 6);
                }
                
                // ����ļ���
                using (var brush = new SolidBrush(System.Drawing.Color.FromArgb(200, System.Drawing.Color.White)))
                {
                    g.FillRectangle(brush, 4, 7, 8, 4);
                }
            }
            var hIcon = bitmap.GetHicon();
            return System.Drawing.Icon.FromHandle(hIcon);
        }

        /// <summary>
        /// ��������ͼ�� (32x32)
        /// </summary>
        /// <returns></returns>
        public static BitmapSource CreateWindowIcon()
        {
            try
            {
                // ����һ��32x32���ص�ͼ�꣬���ڴ��ڱ�����
                var bitmap = new Bitmap(32, 32);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    
                    // ����һ��Բ�α���
                    g.FillEllipse(System.Drawing.Brushes.DodgerBlue, 2, 2, 28, 28);
                    
                    // �����ļ���ͼ������ - �Ŵ�汾
                    using (var pen = new System.Drawing.Pen(System.Drawing.Color.White, 2))
                    {
                        // �ļ�����״ - �������Ŵ�
                        g.DrawRectangle(pen, 6, 12, 20, 12);
                        g.DrawLine(pen, 6, 12, 12, 12);
                        g.DrawLine(pen, 12, 12, 14, 8);
                        g.DrawLine(pen, 14, 8, 20, 8);
                        g.DrawLine(pen, 20, 8, 20, 12);
                    }
                    
                    // ����ļ���
                    using (var brush = new SolidBrush(System.Drawing.Color.FromArgb(200, System.Drawing.Color.White)))
                    {
                        g.FillRectangle(brush, 8, 14, 16, 8);
                    }
                }
                
                var hIcon = bitmap.GetHicon();
                
                // ��System.Drawing.Iconת��ΪWPF ImageSource
                var iconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                    hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
                
                bitmap.Dispose();
                
                return iconSource;
            }
            catch (Exception ex)
            {
                // ���ͼ�괴��ʧ�ܣ�����null
                System.Diagnostics.Debug.WriteLine($"��������ͼ��ʱ����: {ex.Message}");
                return null;
            }
        }
    }
}