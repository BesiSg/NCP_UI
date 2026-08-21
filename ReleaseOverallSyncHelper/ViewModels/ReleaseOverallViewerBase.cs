using NCPBitbucket;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using Utility;
using Utility.Lib.Release;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace ReleaseOverallSyncHelper.ViewModels
{
    public abstract class ReleaseOverallViewerBase<T> : ViewModelBase
        where T : BaseUtility
    {
        protected const string _searchpattern = @"^R(?<Version>\d+\.\d+\.\d+)\.(?<Customer>[A-Za-z]+?)(?<Revision>\d+(?:-[A-Za-z0-9]+)?|-[A-Za-z0-9]+)$";
        private NCPBitbucketHandler _ncpBitBucketHandler;
        private ReleaseOverallParser _releaseOverallParser;
        public ObservableCollection<ReleaseOverallUnitSelection> UnitSelections { get; } = new();
        public ObservableCollection<T> Collection { get; } = new();
        protected List<T> FullContent { get; } = new();

        public AsyncDelegateCommand StartCommand { get; protected set; }
        public DelegateCommand<T> OpenPathCommand { get; protected set; }
        public AsyncDelegateCommand<T> OpenCfgCommand { get; protected set; }
        public AsyncDelegateCommand SearchCommand { get; protected set; }
        public string SearchText
        {
            get => GetValue(() => SearchText);
            set => SetValue(() => SearchText, value);
        }
        protected abstract Task Start();
        public bool CanStart
        {
            get => GetValue(() => CanStart);
            set => SetValue(() => CanStart, value);
        }
        protected abstract void OpenPath(T source);
        protected abstract Task OpenCfg(T source);
        protected abstract Task Search();
        protected void ClearAll()
        {
            FullContent.Clear();
            Collection.Clear();
        }
        protected void UpdateBothList(List<T> source)
        {
            FullContent.AddRange(source);
            Collection.AddRange(source);
        }
        protected void UpdateDisplay(List<T> source)
        {
            Collection.Clear();
            Collection.AddRange(source);
        }
        protected CancellationTokenSource? _readCfgCts;
        public string CfgContent
        {
            get => GetValue(() => CfgContent);
            set => SetValue(() => CfgContent, value);
        }
        public ReleaseOverallViewerBase(SettingHandler<UserAccount> userAccHandler, HttpClient client)
        {
            _ncpBitBucketHandler = new NCPBitbucketHandler(userAccHandler.Get.BBHTMLToken, client);
            CanCloneGit = false;
            Message = string.Empty;
            _releaseOverallParser = new ReleaseOverallParser();
        }
        public AsyncDelegateCommand CloneGitCommand { get; protected set; }
        public DelegateCommand CancelCloneCommand { get; protected set; }
        public DelegateCommand ClearMessageCommand { get; protected set; }
        protected void ClearMessage()
        {
            Message = string.Empty;
        }
        public bool AllSelected
        {
            get => GetValue(() => AllSelected);
            set
            {

                if (SetValue(() => AllSelected, value))
                {
                    foreach (var item in UnitSelections)
                    {
                        item.Selected = value;
                    }
                }
            }
        }
        protected async Task CloneGit()
        {
            CanCloneGit = false;
            try
            {
                var tasks = new List<Task>();
                _cancelcloneCts = new CancellationTokenSource();
                var token = _cancelcloneCts.Token;
                foreach (var unit in UnitSelections)
                {
                    tasks.Add(CloneRepo(unit, token));
                }
                await Task.WhenAll(tasks);
            }
            finally
            {
                CanCloneGit = true;
            }
        }
        public string Message
        {
            get => GetValue(() => Message);
            set => SetValue(() => Message, value);
        }

        public bool CanCloneGit
        {
            get => GetValue(() => CanCloneGit);
            set => SetValue(() => CanCloneGit, value);
        }
        private CancellationTokenSource? _cancelcloneCts;
        private void CancelClone()
        {
            try
            {
                _cancelcloneCts?.Cancel();
                _cancelcloneCts?.Dispose();
            }
            finally
            {
            }
        }
        protected void PopulateUnitSelections(string tag, string cfgContent)
        {
            UnitSelections.Clear();
            UnitSelections.AddRange(_releaseOverallParser.PopulateUnitSelections(tag, cfgContent));
            CanCloneGit = UnitSelections.Count != 0;
        }
        private async Task<string> CloneRepo(ReleaseOverallUnitSelection unit, CancellationToken cancellationToken)
        {
            if (!unit.Selected) return string.Empty;
            var msg = await _ncpBitBucketHandler.Clone("NCP", unit.Version, unit.Unit, unit.ClonePath, true, cancellationToken);
            lock (Message)
            {
                return Message += msg;
            }
        }
        public DelegateCommand<ReleaseOverallUnitSelection> OpenCloneFolderCommand { get; protected set; }
        protected void OpenCloneFolder(ReleaseOverallUnitSelection item)
        {
            if (item == null || string.IsNullOrEmpty(item.ClonePath))
                return;
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{item.ClonePath}\"",
                UseShellExecute = true
            });
        }
    }
}
