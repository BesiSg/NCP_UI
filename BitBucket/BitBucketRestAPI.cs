using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
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
        public async Task<Dictionary<string, Tag>> GetTags(
            string projectkey,
            string reposlug,
            CancellationToken cancellationToken = default)
        {
            var tags = new Dictionary<string, Tag>(StringComparer.OrdinalIgnoreCase);

            int? start = 0;
            bool? isLastPage = false;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            while (!isLastPage.GetValueOrDefault())
            {
                cancellationToken.ThrowIfCancellationRequested();

                string url =
                    $"{_baseURL}/projects/{projectkey}/repos/{reposlug}/tags?start={start}{_limitposURL}";

                var response = await _client.GetAsync(
                    url,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    CheckAndThrowIfError(
                        await response.Content.ReadAsStringAsync(cancellationToken));

                    break;
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                var tagResponse =
                    JsonSerializer.Deserialize<TagResponse>(json, options);

                if (tagResponse?.values == null)
                    break;

                foreach (var tag in tagResponse.values)
                {
                    tags[tag.displayId] = tag;
                }

                isLastPage = tagResponse.isLastPage;
                start = tagResponse.nextPageStart;
            }

            return tags;
        }
        public async Task<List<Commit>> GetCommitsOnBranch(
    string projectkey,
    string reposlug,
    string branchname,
    CancellationToken cancellationToken = default)
        {
            var commits = new List<Commit>();
            int? start = 0;
            bool? isLastPage = false;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            while (isLastPage == false)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string url =
                    $"{_baseURL}/projects/{projectkey}/repos/{reposlug}/commits" +
                    $"?until={Uri.EscapeDataString(branchname)}" +
                    $"&start={start}{_limitposURL}";

                var response =
                    await _client.GetAsync(url, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var json =
                        await response.Content.ReadAsStringAsync(cancellationToken);

                    var commitResponse =
                        JsonSerializer.Deserialize<CommitResponse>(json, options);

                    commits.AddRange(commitResponse.values);

                    isLastPage = commitResponse.isLastPage;
                    start = commitResponse.nextPageStart;
                }
                else
                {
                    CheckAndThrowIfError(
                        await response.Content.ReadAsStringAsync(cancellationToken));
                }
            }

            return commits;
        }
        public async Task<List<Branch>> GetBranchesOnRepository(
           string projectKey,
           string repoSlug,
           CancellationToken cancellationToken = default)
        {
            var allBranches = new List<Branch>();

            int start = 0;
            bool isLastPage = false;

            while (!isLastPage)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string url =
                    $"{_baseURL}/projects/{projectKey}/repos/{repoSlug}/branches" +
                    $"?start={start}&limit={_limitposURL}";

                var response = await _client.GetAsync(url, cancellationToken);

                if ((int)response.StatusCode == 429)
                {
                    var retryAfter =
                    response.Headers.RetryAfter?.Delta ??
                    TimeSpan.FromSeconds(10);

                    await Task.Delay(retryAfter, cancellationToken);

                    response = await _client.GetAsync(url, cancellationToken);
                }

                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                var branchResponse =
                    JsonSerializer.Deserialize<BranchResponse>(json);

                if (branchResponse?.values != null)
                {
                    allBranches.AddRange(branchResponse.values);
                }

                isLastPage = branchResponse?.isLastPage ?? true;

                if (!isLastPage)
                {
                    start = branchResponse.nextPageStart;
                }
            }

            return allBranches;
        }

        public async Task<List<Project>> GetProjects(
    CancellationToken cancellationToken = default)
        {
            int? start = 0;
            bool? isLastPage = false;

            var projects = new List<Project>();

            ClearErrorFlags();

            try
            {
                while (isLastPage == false)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var response =
                        await _client.GetAsync(
                            $"{_baseURL}/projects?start={start}{_limitposURL}",
                            cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        string responseBody =
                            await response.Content.ReadAsStringAsync(cancellationToken);

                        var projectResponse =
                            JsonSerializer.Deserialize<ProjectResponse>(responseBody);

                        projects.AddRange(projectResponse.values);

                        isLastPage = projectResponse.isLastPage;
                        start = projectResponse.nextPageStart;
                    }
                    else
                    {
                        var errorContent =
                            await response.Content.ReadAsStringAsync(cancellationToken);

                        CheckAndThrowIfError(
                            $"Response code {response.IsSuccessStatusCode}, ErrorContent {errorContent}");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CatchAndPromptErr(ex);
            }

            return projects;
        }
        public async Task<List<Repository>> GetRepositoriesOnProject(
    string projectKey,
    CancellationToken cancellationToken = default)
        {
            ClearErrorFlags();

            var repositories = new List<Repository>();

            try
            {
                int start = 0;

                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var response = await _client.GetAsync(
                        $"{_baseURL}/projects/{projectKey}/repos?limit=1000&start={start}",
                        cancellationToken);

                    if (!response.IsSuccessStatusCode)
                    {
                        CheckAndThrowIfError(
                            await response.Content.ReadAsStringAsync(cancellationToken));
                        break;
                    }

                    string responseBody =
                        await response.Content.ReadAsStringAsync(cancellationToken);

                    var repoResponse = JsonSerializer.Deserialize<RepoResponse>(
                        responseBody,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (repoResponse?.values == null)
                        break;

                    repositories.AddRange(repoResponse.values);

                    if (repoResponse.isLastPage)
                        break;

                    start = repoResponse.nextPageStart;
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                CatchAndPromptErr(ex);
            }

            return repositories;
        }

        public async Task<ErrorResult> DownloadFile(
            string project,
            string repo,
            GitRefType refType,
            string branchtag,
            string filename,
            string refSerialNumber,
            List<string> savepaths,
            CancellationToken cancellationToken = default)
        {
            ClearErrorFlags();

            var label =
                refType == GitRefType.Branch
                    ? "heads"
                    : "tags";

            var url =
                $"{_baseURL}/projects/{project}/repos/{repo}/raw/{filename}";

            if (!string.IsNullOrWhiteSpace(branchtag))
            {
                url += $"?at=refs/{label}/{Uri.EscapeDataString(branchtag)}";
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var dir in savepaths)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var dirname = Path.GetDirectoryName(dir);
                    Directory.CreateDirectory(dirname);
                }

                using var response =
                    await _client.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    CheckAndThrowIfError(
                        await response.Content.ReadAsStringAsync(cancellationToken));

                    return Result;
                }

                var content =
                    await response.Content.ReadAsStringAsync(cancellationToken);

                content = content.ReplaceLineEndings("\r\n");

                await Task.WhenAll(
                    savepaths.Select(path =>
                        File.WriteAllTextAsync(
                            path,
                            content,
                            cancellationToken)));
            }
            catch (OperationCanceledException)
            {
                throw;
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
            CancellationToken cancellationToken = default)
        {
            var url =
                $"{_baseURL}/projects/{project}/repos/{repo}/raw/{filename}" +
                $"?at={commitHash}";

            using var response =
                await _client.GetAsync(url, cancellationToken);

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync(cancellationToken);
        }

    }
    public enum GitRefType
    {
        Branch,
        Tag
    }

}
