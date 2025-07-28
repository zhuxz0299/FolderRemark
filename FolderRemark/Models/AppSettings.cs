using System.ComponentModel;

namespace FolderRemark.Models
{
    public class AppSettings : INotifyPropertyChanged
    {
        private double _fontSize = 13;
        private string _theme = "Light";
        private string _lastSelectedPath = string.Empty;

        public double FontSize
        {
            get => _fontSize;
            set
            {
                _fontSize = value;
                OnPropertyChanged(nameof(FontSize));
            }
        }

        public string Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                OnPropertyChanged(nameof(Theme));
            }
        }

        public string LastSelectedPath
        {
            get => _lastSelectedPath;
            set
            {
                _lastSelectedPath = value;
                OnPropertyChanged(nameof(LastSelectedPath));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}