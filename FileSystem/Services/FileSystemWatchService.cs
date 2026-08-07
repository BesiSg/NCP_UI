using System.IO;

namespace FileSystem.Services
{

    public class FileSystemWatchService : IFileSystemWatchService
    {
        private FileSystemWatcher? _watcher;

        public event EventHandler<FileSystemChangedEventArgs>? FileSystemChanged;

        public void Start(string rootPath)
        {
            Stop();

            if (string.IsNullOrWhiteSpace(rootPath))
                throw new ArgumentException("Root path cannot be empty.", nameof(rootPath));

            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException(rootPath);

            _watcher = new FileSystemWatcher(rootPath)
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = false,
                NotifyFilter =
                    NotifyFilters.DirectoryName |
                    NotifyFilters.FileName |
                    NotifyFilters.LastWrite |
                    NotifyFilters.CreationTime
            };

            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
            _watcher.Changed += OnChanged;
            _watcher.Error += OnError;

            _watcher.EnableRaisingEvents = true;
        }

        public void Stop()
        {
            if (_watcher == null)
                return;

            _watcher.EnableRaisingEvents = false;

            _watcher.Created -= OnCreated;
            _watcher.Deleted -= OnDeleted;
            _watcher.Renamed -= OnRenamed;
            _watcher.Changed -= OnChanged;
            _watcher.Error -= OnError;

            _watcher.Dispose();
            _watcher = null;
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            RaiseChanged(e.ChangeType, e.FullPath, null);
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            // After delete, Directory.Exists/File.Exists will return false.
            // We infer directory/file from previous UI tree in ViewModel.
            RaiseChanged(e.ChangeType, e.FullPath, null);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            RaiseChanged(e.ChangeType, e.FullPath, null);
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            RaiseChanged(e.ChangeType, e.FullPath, e.OldFullPath);
        }

        private void RaiseChanged(
            WatcherChangeTypes changeType,
            string fullPath,
            string? oldFullPath)
        {
            bool isDirectory = Directory.Exists(fullPath);

            FileSystemChanged?.Invoke(
                this,
                new FileSystemChangedEventArgs
                {
                    ChangeType = changeType,
                    FullPath = fullPath,
                    OldFullPath = oldFullPath,
                    IsDirectory = isDirectory
                });
        }

        private void OnError(object sender, ErrorEventArgs e)
        {
            // Optional: log this.
            // FileSystemWatcher can raise buffer overflow errors if many events happen quickly.
            var exception = e.GetException();
            System.Diagnostics.Debug.WriteLine(exception);
        }

        public void Dispose()
        {
            Stop();
        }
    }

}
