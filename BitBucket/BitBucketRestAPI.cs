using OfficeOpenXml.FormulaParsing.LexicalAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Utility;
using Utility.Lib.BitBucketRepositories;

namespace BitBucket
{
    public class BitBucketRestAPI : BaseUtility
    {
        private string _baseURL;
        private string _limitposURL;
        private string _token;
        private HttpClient _client;
        public BitBucketRestAPI(string baseurl, string limitposURL, string token, HttpClient client)
        {
            _baseURL = baseurl;
            _limitposURL = limitposURL;
            _token = token;
            _client = client;
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _token);
        }
        public async Task<Dictionary<string, Tag>> GetTags(string projectkey, string reposlug)
        {
            var tags = new Dictionary<string, Tag>();
            var tags2 = new Dictionary<string, Tag>();
            int? start = 0;
            bool? isLastPage = false;
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            while (isLastPage == false)
            {
                string url = $"{_baseURL}/projects/{projectkey}/repos/{reposlug}/tags?start={start}{_limitposURL}";
                var response = await _client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {

                    var json = await response.Content.ReadAsStringAsync();
                    var tagResponse = JsonSerializer.Deserialize<TagResponse>(json, options);
                    tagResponse.values.ForEach(entry => tags[entry.displayId] = entry);

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
        public async Task<List<Commit>> GetCommitsOnBranch(string projectkey, string reposlug, string branchname)
        {
            var commits = new List<Commit>();
            int? start = 0;
            bool? isLastPage = false;
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            while (isLastPage == false)
            {
                string url = $"{_baseURL}/projects/{projectkey}/repos/{reposlug}/commits?until={Uri.EscapeDataString(branchname)}&start={start}{_limitposURL}";
                var response = await _client.GetAsync(url);

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
            return commits;
        }
        public async Task<List<Branch>> GetBranchesOnRepository(string projectKey, string repoSlug)//string projectKey, string repoSlug
        {
            ClearErrorFlags();
            var allBranches = new List<Branch>();
            int start = 0;
            bool isLastPage = false;
            try
            {
                while (!isLastPage)
                {
                    string url = $"{_baseURL}/projects/{projectKey}/repos/{repoSlug}/branches?start={start}";

                    HttpResponseMessage response = await _client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        CheckAndThrowIfError($"Status code: {response.StatusCode}, Error response: " + errorContent);
                        break;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    var branchResponse = JsonSerializer.Deserialize<BranchResponse>(json);

                    allBranches.AddRange(branchResponse.values);

                    isLastPage = branchResponse.isLastPage;
                    start = branchResponse.start + branchResponse.size;
                }
            }
            catch (Exception ex)
            {
                CatchAndPromptErr(ex);
            }
            return allBranches;
        }
        public async Task<List<Project>> GetProjects()
        {
            int? start = 0;
            bool? isLastPage = false;
            var projects = new List<Project>();
            ClearErrorFlags();
            var listofRepos = new List<string>();
            try
            {
                while (isLastPage == false)
                {
                    // Send GET request
                    var response = await _client.GetAsync($"{_baseURL}/projects?start={start}{_limitposURL}");

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var projectResponse = JsonSerializer.Deserialize<ProjectResponse>(responseBody);
                        projects.AddRange(projectResponse.values);
                        isLastPage = projectResponse.isLastPage;
                        start = projectResponse.nextPageStart;
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        CheckAndThrowIfError($"Response code {response.IsSuccessStatusCode}, ErrorContent {errorContent}");
                    }
                }
            }
            catch (Exception ex)
            {
                CatchAndPromptErr(ex);
            }
            return projects;
        }
        public async Task<List<Repository>> GetRepositoriesOnProject(string projectKey)
        {
            ClearErrorFlags();
            var listofRepos = new List<Repository>();
            try
            {
                // Send GET request
                HttpResponseMessage response = await _client.GetAsync($"{_baseURL}/projects/{projectKey}/repos?{_limitposURL}");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var repoResponse = JsonSerializer.Deserialize<RepoResponse>(responseBody);
                    listofRepos.AddRange(repoResponse.values);
                }
                else
                {
                    CheckAndThrowIfError(await response.Content.ReadAsStringAsync());
                }
            }
            catch (Exception ex)
            {
                CatchAndPromptErr(ex);
            }
            return listofRepos;
        }
        public async Task<ErrorResult> DownloadFile(
            string project,
            string repo,
            GitRefType refType,
            string branchtag,
            string filename,
            string refSerialNumber,
            List<string> savepaths)
        {
            ClearErrorFlags();

            var label = refType == GitRefType.Branch
                ? "heads"
                : "tags";

            var url = $"{_baseURL}/projects/{project}/repos/{repo}/raw/{filename}";
            if (!string.IsNullOrWhiteSpace(branchtag))
            {
                url += $"?at=refs/{label}/{Uri.EscapeDataString(branchtag)}";
            }
            try
            {
                foreach (var dir in savepaths)
                {
                    var dirname = Path.GetDirectoryName(dir);
                    Directory.CreateDirectory(dirname);
                }

                using var response = await _client.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    CheckAndThrowIfError(await response.Content.ReadAsStringAsync());
                    return Result;
                }
                var content = await response.Content.ReadAsStringAsync();
                content = content.ReplaceLineEndings("\r\n");
                await Task.WhenAll(savepaths.Select(path =>File.WriteAllTextAsync(path, content)));
            }
            catch (Exception ex)
            {
                CatchAndPromptErr(ex);
            }
            return Result;
        }
        public async Task<string> GetFileContent(
            string project,
            string repo,
            string filename,
            string commitHash,
            CancellationToken token)
        {
            var url =
                $"{_baseURL}/projects/{project}/repos/{repo}/raw/{filename}" +
                $"?at={commitHash}";

            using var response =
                await _client.GetAsync(url, token);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(token);
        }
    }
    public enum GitRefType
    {
        Branch,
        Tag
    }

}
