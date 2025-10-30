using BitBucketHandler;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Utility.Lib.BitBucketRepositories;

namespace BitBucket
{
    public class BitBucketRepositoryHandler(string token, BitBucketStorage<Repository> dataset) : BitBucket<BitBucketStorage<Repository>>(token, dataset), iBitBucketHandler
    {
        public async Task GetAllAsync(string key)//string projectKey
        {
            var listofRepos = new List<Repository>();
            using (HttpClient client = new HttpClient())
            {
                // Add Authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HTTPaccesstoken);

                try
                {
                    // Send GET request
                    HttpResponseMessage response = await client.GetAsync($"{_baseURL}/projects/{key}/repos?{limitposURL}");

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
                    CatchException(ex);
                }
            }
            lock (_dataset)
            {
                _dataset.Update(listofRepos, key);
            }
        }
    }
}
