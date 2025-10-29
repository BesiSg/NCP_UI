using System.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Utility.Lib.BitBucketRepositories;

namespace BitBucket
{
    public class BitBucketTagHandler(string token, BitBucketStorage<Tag> dataset, BitBucketStorage<Commit> datasetcommit) : BitBucket(token)
    {
        protected BitBucketStorage<Tag> _dataset = dataset;
        protected BitBucketStorage<Commit> _datasetcommit = datasetcommit;
        public (Tag, string) GetLatestTagAndNext(string projectkey, string reposlug, string branchname)
        {
            var key = $"{projectkey} {reposlug} {branchname}";
            if (!_dataset.Storage.ContainsKey(key)) return (null, string.Empty);
            var latest = _dataset.Storage[key].FirstOrDefault(tag => tag.displayId != string.Empty && tag.displayId != null);
            return (latest, GetNextTag(latest.displayId));
        }
        private string GetNextTag(string latestTag)
        {
            if (latestTag == null) return string.Empty;
            // Match consecutive digits
            Match digitsMatch = Regex.Match(latestTag, @"\d+$");

            string digits = digitsMatch.Success ? digitsMatch.Value : string.Empty;
            var tag = latestTag.Remove(latestTag.Count() - digits.Length);
            if (digits == string.Empty) return string.Empty;
            var nextrev = int.Parse(digits) + 1;
            return tag + nextrev.ToString();
        }
        public async Task GetTags(string projectkey, string reposlug, string branchname)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HTTPaccesstoken);

            await GetAllCommitsOnBranchAsync(httpClient, projectkey, reposlug, branchname);
            var allTags = await GetAllTagsAsync(httpClient, projectkey, reposlug);
            var tagsOnBranch = new List<Tag>();
            var commits = _datasetcommit.Storage[$"{projectkey} {reposlug} {branchname}"];

            foreach (var commit in commits)
            {
                if (allTags.ContainsKey(commit.id))
                {
                    tagsOnBranch.Add(allTags[commit.id]);
                }
                else
                    tagsOnBranch.Add(new Tag() { latestCommit = commit.id });
            }
            tagsOnBranch.OrderByDescending(id => id.displayId);
            lock (_dataset)
            {
                _dataset.Update(tagsOnBranch, $"{projectkey} {reposlug} {branchname}");
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
            lock (_datasetcommit)
            {
                _datasetcommit.Update(commits, $"{projectkey} {reposlug} {branchname}");
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
                    //tags.AddRange(tagResponse.values);

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
