using BitBucketHandler;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Storage;

namespace BitBucket
{
    public class BitBucketTagHandler(string token, StorageDat<Commit> dataset, HttpClient client) : BitBucket<StorageDat<Commit>>(token, dataset, client), iBitBucketHandler
    {
        public (Commit, string) GetLatestTagAndNext(string projectkey, string reposlug, string branchname)
        {
            var key = $"{projectkey} {reposlug} {branchname}";
            if (!_dataset.Storage.ContainsKey(key)) return (null, string.Empty);
            var latest = _dataset.Storage[key].FirstOrDefault(tag => tag.displayId != string.Empty && tag.displayId != null && tag.latestCommit != string.Empty && tag.latestCommit != null);
            if (latest == null) return (null, string.Empty);
            return (latest, GetNextTag(latest.displayId));
        }
        private string GetNextTag(string latestTag)
        {
            if (latestTag == null) return string.Empty;
            var parts = latestTag.Split('.');
            if (parts.Length != 4) return string.Empty;
            // Match consecutive digits
            Match digitsMatch = Regex.Match(latestTag, @"\d+$");
            string digits = digitsMatch.Success ? digitsMatch.Value : string.Empty;
            var tag = latestTag.Remove(latestTag.Count() - digits.Length);
            if (digits == string.Empty) return string.Empty;
            var nextrev = int.Parse(digits) + 1;
            return tag + nextrev.ToString();
        }
        public async Task<ErrorResult> GetAllAsync(string key)
        {
            ClearErrorFlags();
            var text = key.Split(' ');
            try
            {
                if (text.Length != 3) CheckAndThrowIfError($"Expected Key length =3, actual key length {text.Length}");
            }
            catch (Exception e) { CatchAndPromptErr(e); }
            if (NoError())
            {
                var projectkey = text[0];
                var reposlug = text[1];
                var branchname = text[2];
                var taskgetcommits = GetAllCommitsOnBranchAsync(projectkey, reposlug, branchname);
                var taskgettags = _bitBucket.GetTags(projectkey, reposlug);
                await Task.WhenAll(taskgetcommits, taskgettags);
                var tagsOnBranch = new List<Tag>();
                var commits = _dataset.Storage[$"{projectkey} {reposlug} {branchname}"];
                lock (_dataset)
                {
                    foreach (var commit in commits)
                    {
                        if (taskgettags.Result.ContainsKey(commit.id))
                        {
                            commit.UpdateTag(taskgettags.Result[commit.id]);
                        }
                    }
                }
            }
            return Result;
        }
        private async Task GetAllCommitsOnBranchAsync(string projectkey, string reposlug, string branchname)
        {
            var commits = await _bitBucket.GetCommitsOnBranch(projectkey, reposlug, branchname);
            lock (_dataset)
            {
                _dataset.Update(commits, $"{projectkey} {reposlug} {branchname}");
            }
        }
    }
}
