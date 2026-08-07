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
    public abstract class ReleaseOverallViewerBase : ViewModelBase
    {
        private string _searchpattern = string.Empty;
        private NCPBitbucketHandler _ncpBitBucketHandler;
        public ObservableCollection<ReleaseOverallUnitSelection> UnitSelections { get; } = new();
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

        private const string EndMarker =
            "# This is a dummy entry to satisfy the CopyRelease script";

        private static readonly Regex UnitRegex =
            new(
                @"^(?<Unit>[A-Z0-9]{4}|Version_MMI)\s*(?:=)?\s+(?<Version>[VR]\S+)",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase |
                RegexOptions.Multiline);
        protected void PopulateUnitSelections(string tag, string cfgContent)
        {
            UnitSelections.Clear();

            // Ignore everything after the marker
            int markerIndex = cfgContent.IndexOf(
                EndMarker,
                StringComparison.OrdinalIgnoreCase);

            if (markerIndex >= 0)
            {
                cfgContent = cfgContent[..markerIndex];
            }

            foreach (Match match in UnitRegex.Matches(cfgContent))
            {
                var item = new ReleaseOverallUnitSelection(
                        tag,
                        match.Groups["Unit"].Value,
                        match.Groups["Version"].Value);
                UnitSelections.Add(item);
            }
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
