using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FolderRemark.Services
{
    public class FolderWatcher : IDisposable
    {
        private FileSystemWatcher _watcher;
        private string _watchedPath;

        public event Action<string> FolderAdded;
        public event Action<string> FolderDeleted;

        public void StartWatching(string path)
        {
            StopWatching();
            
            if (!Directory.Exists(path))
                return;

            _watchedPath = path;
            _watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.DirectoryName,
                EnableRaisingEvents = true
            };

            _watcher.Created += OnFolderCreated;
            _watcher.Deleted += OnFolderDeleted;
        }

        public void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Created -= OnFolderCreated;
                _watcher.Deleted -= OnFolderDeleted;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnFolderCreated(object sender, FileSystemEventArgs e)
        {
            if (Directory.Exists(e.FullPath))
            {
                FolderAdded?.Invoke(e.FullPath);
            }
        }

        private void OnFolderDeleted(object sender, FileSystemEventArgs e)
        {
            FolderDeleted?.Invoke(e.FullPath);
        }

        public void Dispose()
        {
            StopWatching();
        }
    }
}