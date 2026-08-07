using System.Collections.ObjectModel;
using System.IO;
using Utility;

namespace FileSystem.ViewModels
{
    public class FileSystemNodeViewModel : BaseUtility
    {
        public FileSystemNodeViewModel()
        {
        }

        public FileSystemNodeViewModel(string fullPath, bool isDirectory)
        {
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath);

            if (string.IsNullOrWhiteSpace(Name))
                Name = fullPath;

            IsDirectory = isDirectory;
        }

        public string Name
        {
            get => GetValue(() => Name);
            set => SetValue(() => Name, value);
        }

        public string FullPath
        {
            get => GetValue(() => FullPath);
            set => SetValue(() => FullPath, value);
        }

        public bool IsDirectory
        {
            get => GetValue(() => IsDirectory);
            set => SetValue(() => IsDirectory, value);
        }

        public bool IsExpanded
        {
            get => GetValue(() => IsExpanded);
            set => SetValue(() => IsExpanded, value);
        }

        public bool IsSelected
        {
            get => GetValue(() => IsSelected);
            set => SetValue(() => IsSelected, value);
        }

        public ObservableCollection<FileSystemNodeViewModel> Children { get; } = new();
    }

}
