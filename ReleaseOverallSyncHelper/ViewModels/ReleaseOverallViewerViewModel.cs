using FileSearcher;
using NCPBitbucket;
using ReleaseOverallSyncHelper.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using Utility;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace ReleaseOverallSyncHelper.ViewModels
{
    public class ReleaseOverallViewerViewModel : ReleaseOverallViewerBase
    {
        private string _rootPath = string.Empty;
        private string _searchpattern = string.Empty;
        private FolderSearcherHelper _foldersearcher;
        public ObservableCollection<PathName> Collection { get; } = new();
        public ReleaseOverallViewerViewModel(string path, string searchpattern, SettingHandler<UserAccount> userAccHandler, HttpClient client) : base(userAccHandler, client)
        {
            _rootPath = path;
            _searchpattern = searchpattern;
            _foldersearcher = new FolderSearcherHelper(_rootPath, string.Empty, "patch", string.Empty, ".cfg", _searchpattern);
            CanStart = true;
            Message = string.Empty;
        }
        public AsyncDelegateCommand StartCommand { get; protected set; }
        public AsyncDelegateCommand<PathName> OpenCfgCommand { get; protected set; }
        private async Task Start()
        {
            CanStart = false;
            try
            {
                Collection.Clear();
                await _foldersearcher.Search();
                PopulatePath();
            }
            finally
            {
                CanStart = true;
            }
        }
        public bool CanStart
        {
            get => GetValue(() => CanStart);
            set => SetValue(() => CanStart, value);
        }
        private void HookSelectionChanged(ReleaseOverallUnitSelection item)
        {
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(ReleaseOverallUnitSelection.Selected))
                {
                    AllSelected = UnitSelections.All(x => x.Selected);
                }
            };
        }
        private CancellationTokenSource? _readCfgCts;
        private async Task OpenCfg(PathName item)
        {
            selectedPath = item;
            if (item == null || string.IsNullOrWhiteSpace(item.Fullpath))
                return;

            if (!File.Exists(item.Fullpath))
                return;

            // Cancel previous file loading if still running
            _readCfgCts?.Cancel();
            _readCfgCts?.Dispose();

            _readCfgCts = new CancellationTokenSource();
            var token = _readCfgCts.Token;

            try
            {
                CfgContent = "Loading...";

                string content = await File.ReadAllTextAsync(item.Fullpath, token);

                // Only update if this task was not cancelled
                token.ThrowIfCancellationRequested();

                CfgContent = content;
                PopulateUnitSelections(Path.GetFileNameWithoutExtension(selectedPath.Fullpath), CfgContent);
            }
            catch (OperationCanceledException)
            {
                // Ignore because user selected another cfg file
            }
            catch (Exception ex)
            {
                CfgContent = $"Failed to read file:\r\n{ex.Message}";
            }
        }
        private PathName selectedPath;
        private const string EndMarker =
            "# This is a dummy entry to satisfy the CopyRelease script";

        private static readonly Regex UnitRegex =
            new(
                @"^(?<Unit>[A-Z0-9]{4}|Version_MMI)\s*(?:=)?\s+(?<Version>[VR]\S+)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.Multiline);
        private void PopulatePath()
        {
            Collection.Clear();
            var keys = _foldersearcher.ReleaseOverall.Keys.OrderBy(x => x, new NaturalSortComparer());

            foreach (var key in keys)
                Collection.AddRange(_foldersearcher.ReleaseOverall[key].OrderBy(x => x.Name, new NaturalSortComparer()));
        }
        public DelegateCommand<PathName> OpenFolderCommand { get; private set; }
        public bool CanOpenFolder => true;
        private void OpenFolder(PathName item)
        {
            if (item == null || string.IsNullOrEmpty(item.Fullpath))
                return;
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{item.Fullpath}\"",
                UseShellExecute = true
            });
        }
    }
}
