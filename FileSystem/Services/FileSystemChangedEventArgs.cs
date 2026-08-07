using System.IO;

namespace FileSystem.Services
{
    public class FileSystemChangedEventArgs : EventArgs
    {
        public WatcherChangeTypes ChangeType { get; set; }

        public string FullPath { get; set; } = string.Empty;

        public string? OldFullPath { get; set; }

        public bool IsDirectory { get; set; }
    }

}
