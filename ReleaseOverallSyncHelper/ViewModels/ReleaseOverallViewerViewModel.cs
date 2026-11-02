using FileSearcher;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using Utility;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace ReleaseOverallSyncHelper.ViewModels
{
    public class ReleaseOverallViewerViewModel : ReleaseOverallViewerBase<PathName>
    {
        private string _rootPath = string.Empty;
        private FolderSearcherHelper _foldersearcher;
        public ReleaseOverallViewerViewModel(string path, SettingHandler<UserAccount> userAccHandler, HttpClient client) : base(userAccHandler, client)
        {
            _rootPath = path;
            _foldersearcher = new FolderSearcherHelper(_rootPath, string.Empty, ["patch", "rc2-CDE"], string.Empty, ".cfg", _searchpattern);
            CanStart = true;
            Message = string.Empty;
            SearchText = string.Empty;
        }
        protected override async Task Start()
        {
            CanStart = false;
            try
            {
                SearchText = string.Empty;
                ClearAll();
                await _foldersearcher.Search();
                PopulatePath();
            }
            finally
            {
                CanStart = true;
            }
        }
        protected override void OpenPath(PathName source)
        {
            if (source == null || string.IsNullOrEmpty(source.Fullpath))
                return;
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{source.Fullpath}\"",
                UseShellExecute = true
            });
        }
        protected override async Task OpenCfg(PathName source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.Fullpath))
                return;

            if (!File.Exists(source.Fullpath))
                return;

            // Cancel previous file loading if still running
            _readCfgCts?.Cancel();
            _readCfgCts?.Dispose();

            _readCfgCts = new CancellationTokenSource();
            var token = _readCfgCts.Token;

            try
            {
                CfgContent = "Loading...";

                string content = await File.ReadAllTextAsync(source.Fullpath, token);

                // Only update if this task was not cancelled
                token.ThrowIfCancellationRequested();

                CfgContent = content;
                PopulateUnitSelections(Path.GetFileNameWithoutExtension(source.Fullpath), CfgContent);
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
        private void PopulatePath()
        {
            var keys = _foldersearcher.ReleaseOverall.Keys.OrderBy(x => x, new NaturalSortComparer());

            foreach (var key in keys)
                UpdateBothList(_foldersearcher.ReleaseOverall[key].OrderBy(x => x.Name, new NaturalSortComparer()).ToList());
        }
        protected override async Task Search()
        {
            var filtered = await Task.Run(() => FullContent.Where(content => content.Name.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase)).ToList());
            UpdateDisplay(filtered);
        }
    }
}
