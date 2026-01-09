using BitBucketHandler;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using Utility.Lib.BitBucketRepositories;

namespace BitBucket
{
    public class BitBucketTagHandler(string token, BitBucketStorage<Commit> dataset) : BitBucket<BitBucketStorage<Commit>>(token, dataset), iBitBucketHandler
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
        public async Task GetAllAsync(string key)
        {
            var text = key.Split(' ');
            if (text.Length != 3) return;
            var projectkey = text[0];
            var reposlug = text[1];
            var branchname = text[2];
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HTTPaccesstoken);

            await GetAllCommitsOnBranchAsync(httpClient, projectkey, reposlug, branchname);
            var allTags = await GetAllTagsAsync(httpClient, projectkey, reposlug);
            var tagsOnBranch = new List<Tag>();

            var commits = _dataset.Storage[$"{projectkey} {reposlug} {branchname}"];
            lock (_dataset)
            {
                foreach (var commit in commits)
                {
                    if (allTags.ContainsKey(commit.id))
                    {
                        commit.UpdateTag(allTags[commit.id]);
                        //tagsOnBranch.Add(allTags[commit.id]);
                    }
                    //else
                    //    tagsOnBranch.Add(new Tag() { latestCommit = commit.id });
                }
                //tagsOnBranch.OrderByDescending(id => id.displayId);

                //_dataset.Update(tagsOnBranch, $"{projectkey} {reposlug} {branchname}");
            }
        }

        private async Task GetAllCommitsOnBranchAsync(HttpClient client, string projectkey, string reposlug, string branchname)
        {
            var commits = new List<Commit>();
            int? start = 0;
            bool? isLastPage = false;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            while (isLastPage == false)
            {
                string url = $"{_baseURL}/projects/{projectkey}/repos/{reposlug}/commits?until={Uri.EscapeDataString(branchname)}&start={start}{limitposURL}";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var commitResponse = JsonSerializer.Deserialize<CommitResponse>(json, options);

                    commits.AddRange(commitResponse.values);

                    isLastPage = commitResponse.isLastPage;
                    start = commitResponse.nextPageStart;
                }
                else
                {
                    CheckAndThrowIfError(await response.Content.ReadAsStringAsync());
                }
            }
            lock (_dataset)
            {
                _dataset.Update(commits, $"{projectkey} {reposlug} {branchname}");
            }
        }
        private async Task<Dictionary<string, Tag>> GetAllTagsAsync(HttpClient client, string projectkey, string reposlug)
        {
            var tags = new Dictionary<string, Tag>();
            var tags2 = new Dictionary<string, Tag>();
            int? start = 0;
            bool? isLastPage = false;

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            while (isLastPage == false)
            {
                string url = $"{_baseURL}/projects/{projectkey}/repos/{reposlug}/tags?start={start}{limitposURL}";
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {

                    var json = await response.Content.ReadAsStringAsync();
                    var tagResponse = JsonSerializer.Deserialize<TagResponse>(json, options);
                    tagResponse.values.ForEach(entry => tags[entry.latestCommit] = entry);

                    isLastPage = tagResponse.isLastPage;
                    start = tagResponse.nextPageStart;
                }
                else
                {
                    CheckAndThrowIfError(await response.Content.ReadAsStringAsync());
                }
            }
            return tags;
        }
    }
}
