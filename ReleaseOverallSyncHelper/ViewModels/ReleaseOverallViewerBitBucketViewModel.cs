using BitBucket;
using FileSearcher;
using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Unity.Injection;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace ReleaseOverallSyncHelper.ViewModels
{
    public class ReleaseOverallViewerBitBucketViewModel : ReleaseOverallViewerBase
    {
        const string pattern = @"^R(?<Version>\d+\.\d+\.\d+)\.(?<Customer>[A-Za-z]+?)(?<Revision>\d+(?:-[A-Za-z0-9]+)?|-[A-Za-z0-9]+)$";
        private BitBucketRestAPI _bitBucketRestAPI;
        protected const string _baseURL = "http://bb.eu.besi.corp/rest/api/1.0";
        protected const string _limitposURL = "&limit=1000";
        public ObservableCollection<Tag> Collection { get; } = new();
        public List<Tag> FullCollection { get; } = new();
        public ReleaseOverallViewerBitBucketViewModel(SettingHandler<UserAccount> userAccHandler, HttpClient client) : base(userAccHandler, client)
        {
            _bitBucketRestAPI = new BitBucketRestAPI(_baseURL, _limitposURL, userAccHandler.Get.BBHTMLToken, client);
            CanStart = true;
        }
        public AsyncDelegateCommand StartCommand { get; protected set; }
        public AsyncDelegateCommand<Tag> OpenCommitCommand { get; protected set; }
        public AsyncDelegateCommand<Tag> OpenCfgCommand { get; protected set; }

        private async Task Start()
        {
            CanStart = false;
            try
            {
                Collection.Clear();
                var listoftags = new List<Tag>();
                var tags = await _bitBucketRestAPI.GetTags("ncp", "release_overall");
                var filteredtags = new Dictionary<string, List<Tag>>();
                foreach (var tag in tags)
                {
                    var match = Regex.Match(tag.Key, pattern, RegexOptions.IgnoreCase);
                    if (!match.Success)
                        continue;
                    var key = $"support/{match.Groups["Customer"].ToString().ToLower()}/{match.Groups["Version"]}";
                    if (!filteredtags.ContainsKey(key)) filteredtags[key] = new List<Tag>();
                    filteredtags[key].Add(tag.Value);
                }
                var keys = filteredtags.Keys.OrderBy(x => x, new NaturalSortComparer());
                foreach (var key in keys)
                    listoftags.AddRange(filteredtags[key].OrderBy(x => x.displayId, new NaturalSortComparer()));
                PopulatePath(listoftags);
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
        private CancellationTokenSource? _readCfgCts;
        private async Task OpenCommit(Tag item)
        {
            await Task.Run(() =>
            {
                if (item == null || string.IsNullOrWhiteSpace(item.latestCommit))
                    return;
                var url = $"http://bb.eu.besi.corp/projects/NCP/repos/release_overall/commits/{item.latestCommit}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            });
        }
        private async Task OpenCfg(Tag item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.latestCommit))
                return;

            // Cancel previous file loading if still running
            _readCfgCts?.Cancel();
            _readCfgCts?.Dispose();

            _readCfgCts = new CancellationTokenSource();
            var token = _readCfgCts.Token;

            try
            {
                CfgContent = "Loading...";

                CfgContent = await _bitBucketRestAPI.GetFileContent(
                    "NCP",
                    "release_overall",
                    "Release.cfg",
                    item.latestCommit,
                    _readCfgCts.Token);
                PopulateUnitSelections(item.displayId, CfgContent);
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

        private void PopulatePath(List<Tag> tags)
        {
            Collection.Clear();
            Collection.AddRange(tags);
        }
    }
}
