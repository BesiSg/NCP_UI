namespace FileSystem.Services
{
    public interface IFileSystemWatchService : IDisposable
    {
        event EventHandler<FileSystemChangedEventArgs>? FileSystemChanged;

        void Start(string rootPath);

        void Stop();
    }

}
