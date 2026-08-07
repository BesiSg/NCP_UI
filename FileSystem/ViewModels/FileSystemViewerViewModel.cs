using FileSystem.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Utility;

namespace FileSystem.ViewModels
{
    public class FileSystemViewerViewModel : ViewModelBase
    {

        private readonly IFileSystemWatchService _watchService;

        private string _rootPath = @"D:\NCP\Machines";
        private string _statusMessage = string.Empty;

        public FileSystemViewerViewModel(IFileSystemWatchService watchService)
        {
            _watchService = watchService;
            _watchService.FileSystemChanged += OnFileSystemChanged;
        }

        public ObservableCollection<FileSystemNodeViewModel> RootNodes { get; } = new();


        public DelegateCommand LoadedCommand { get; protected set; }
        public DelegateCommand UnloadedCommand { get; protected set; }
        public DelegateCommand RefreshCommand { get; protected set; }
        public DelegateCommand<FileSystemNodeViewModel> OpenInExplorerCommand { get; protected set; }

        public string RootPath
        {
            get => _rootPath;
            set => SetProperty(ref _rootPath, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }
        public bool CanOpenInExplorer => true;
        private void Loaded()
        {
            StartWatch();
        }

        private void Unloaded()
        {
            StopWatch();
        }

        private void OpenInExplorer(FileSystemNodeViewModel? node)
        {
            if (node == null)
                return;

            if (!File.Exists(node.FullPath) &&
                !Directory.Exists(node.FullPath))
                return;

            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{node.FullPath}\"",
                UseShellExecute = true
            });
        }

        private void StartWatch()
        {
            try
            {
                if (!Directory.Exists(RootPath))
                {
                    StatusMessage = $"Folder does not exist: {RootPath}";
                    return;
                }

                LoadInitialTree();

                _watchService.Start(RootPath);

                StatusMessage = $"Watching: {RootPath}";
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }

        private void StopWatch()
        {
            _watchService.Stop();
            StatusMessage = "Watcher stopped.";
        }

        private void Refresh()
        {
            LoadInitialTree();
            StatusMessage = "Tree refreshed.";
        }

        private void LoadInitialTree()
        {
            RootNodes.Clear();

            if (!Directory.Exists(RootPath))
                return;

            var rootNode = BuildNode(RootPath);
            rootNode.IsExpanded = true;

            RootNodes.Add(rootNode);
        }

        private FileSystemNodeViewModel BuildNode(string path)
        {
            var node = new FileSystemNodeViewModel(path, Directory.Exists(path));

            if (!node.IsDirectory)
                return node;

            try
            {
                foreach (var directory in Directory.GetDirectories(path).OrderBy(x => x))
                {
                    node.Children.Add(BuildNode(directory));
                }

                foreach (var file in Directory.GetFiles(path).OrderBy(x => x))
                {
                    node.Children.Add(new FileSystemNodeViewModel(file, false));
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Optional: add placeholder node or log.
            }
            catch (IOException)
            {
                // Optional: path may disappear while loading.
            }

            return node;
        }

        private void OnFileSystemChanged(object? sender, FileSystemChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    switch (e.ChangeType)
                    {
                        case WatcherChangeTypes.Created:
                            AddNode(e.FullPath);
                            StatusMessage = $"Created: {e.FullPath}";
                            break;

                        case WatcherChangeTypes.Deleted:
                            RemoveNode(e.FullPath);
                            StatusMessage = $"Deleted: {e.FullPath}";
                            break;

                        case WatcherChangeTypes.Renamed:
                            if (!string.IsNullOrWhiteSpace(e.OldFullPath))
                            {
                                RenameNode(e.OldFullPath, e.FullPath);
                                StatusMessage = $"Renamed: {e.OldFullPath} -> {e.FullPath}";
                            }
                            break;

                        case WatcherChangeTypes.Changed:
                            StatusMessage = $"Changed: {e.FullPath}";
                            break;
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = ex.Message;
                }
            });
        }

        private void AddNode(string fullPath)
        {
            if (FindNode(fullPath) != null)
                return;

            string? parentPath = Path.GetDirectoryName(fullPath);

            if (string.IsNullOrWhiteSpace(parentPath))
                return;

            var parentNode = FindNode(parentPath);

            if (parentNode == null)
            {
                Refresh();
                return;
            }

            bool isDirectory = Directory.Exists(fullPath);
            bool isFile = File.Exists(fullPath);

            if (!isDirectory && !isFile)
                return;

            var newNode = new FileSystemNodeViewModel(fullPath, isDirectory);

            InsertSorted(parentNode.Children, newNode);

            parentNode.IsExpanded = true;
        }

        private void RemoveNode(string fullPath)
        {
            var node = FindNode(fullPath);

            if (node == null)
                return;

            var parent = FindParentNode(fullPath);

            if (parent == null)
            {
                RootNodes.Remove(node);
            }
            else
            {
                parent.Children.Remove(node);
            }
        }

        private void RenameNode(string oldFullPath, string newFullPath)
        {
            var node = FindNode(oldFullPath);

            if (node == null)
            {
                AddNode(newFullPath);
                return;
            }

            node.FullPath = newFullPath;
            node.Name = Path.GetFileName(newFullPath);
            node.IsDirectory = Directory.Exists(newFullPath);

            UpdateChildrenPaths(node, oldFullPath, newFullPath);
        }

        private void UpdateChildrenPaths(
            FileSystemNodeViewModel node,
            string oldBasePath,
            string newBasePath)
        {
            foreach (var child in node.Children)
            {
                if (child.FullPath.StartsWith(oldBasePath, StringComparison.OrdinalIgnoreCase))
                {
                    child.FullPath = newBasePath + child.FullPath.Substring(oldBasePath.Length);
                    child.Name = Path.GetFileName(child.FullPath);
                }

                UpdateChildrenPaths(child, oldBasePath, newBasePath);
            }
        }

        private FileSystemNodeViewModel? FindNode(string fullPath)
        {
            foreach (var root in RootNodes)
            {
                var result = FindNodeRecursive(root, fullPath);

                if (result != null)
                    return result;
            }

            return null;
        }

        private FileSystemNodeViewModel? FindNodeRecursive(
            FileSystemNodeViewModel current,
            string fullPath)
        {
            if (string.Equals(current.FullPath, fullPath, StringComparison.OrdinalIgnoreCase))
                return current;

            foreach (var child in current.Children)
            {
                var result = FindNodeRecursive(child, fullPath);

                if (result != null)
                    return result;
            }

            return null;
        }

        private FileSystemNodeViewModel? FindParentNode(string fullPath)
        {
            string? parentPath = Path.GetDirectoryName(fullPath);

            if (string.IsNullOrWhiteSpace(parentPath))
                return null;

            return FindNode(parentPath);
        }

        private void InsertSorted(
            ObservableCollection<FileSystemNodeViewModel> collection,
            FileSystemNodeViewModel newNode)
        {
            int index = 0;

            while (index < collection.Count)
            {
                var current = collection[index];

                if (newNode.IsDirectory && !current.IsDirectory)
                    break;

                if (newNode.IsDirectory == current.IsDirectory &&
                    string.Compare(newNode.Name, current.Name, StringComparison.OrdinalIgnoreCase) < 0)
                    break;

                index++;
            }

            collection.Insert(index, newNode);
        }

        public void Dispose()
        {
            _watchService.FileSystemChanged -= OnFileSystemChanged;
            _watchService.Dispose();
        }
    }
}
