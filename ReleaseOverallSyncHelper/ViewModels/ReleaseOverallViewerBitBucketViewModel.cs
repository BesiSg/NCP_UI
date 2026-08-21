using BitBucket;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.SettingHandler;
using Utility.Lib.UserAccount;

namespace ReleaseOverallSyncHelper.ViewModels
{
    public class ReleaseOverallViewerBitBucketViewModel : ReleaseOverallViewerBase<Tag>
    {
        private BitBucketRestAPI _bitBucketRestAPI;
        protected const string _baseURL = "http://bb.eu.besi.corp/rest/api/1.0";
        protected const string _limitposURL = "&limit=1000";
        public ReleaseOverallViewerBitBucketViewModel(SettingHandler<UserAccount> userAccHandler, HttpClient client) : base(userAccHandler, client)
        {
            _bitBucketRestAPI = new BitBucketRestAPI(_baseURL, _limitposURL, userAccHandler.Get.BBHTMLToken, client);
            CanStart = true;
            SearchText = string.Empty;
        }
        protected override async Task Start()
        {
            CanStart = false;
            try
            {
                SearchText = string.Empty;
                ClearAll();
                var listoftags = new List<Tag>();
                var tags = await _bitBucketRestAPI.GetTags("ncp", "release_overall");
                var filteredtags = new Dictionary<string, List<Tag>>();
                foreach (var tag in tags)
                {
                    var match = Regex.Match(tag.Key, _searchpattern, RegexOptions.IgnoreCase);
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
        protected override void OpenPath(Tag source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.latestCommit))
                return;
            var url = $"http://bb.eu.besi.corp/projects/NCP/repos/release_overall/commits/{source.latestCommit}";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        protected override async Task OpenCfg(Tag source)
        {
            if (source == null || string.IsNullOrWhiteSpace(source.latestCommit))
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
                    source.latestCommit,
                    _readCfgCts.Token);
                PopulateUnitSelections(source.displayId, CfgContent);
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
            UpdateBothList(tags);
        }
        protected override async Task Search()
        {
            var filtered = await Task.Run(() => FullContent.Where(content => content.displayId.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase)).ToList());
            UpdateDisplay(filtered);
        }
    }
}
