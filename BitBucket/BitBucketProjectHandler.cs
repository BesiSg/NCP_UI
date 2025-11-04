using BitBucketHandler;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Utility.Lib.BitBucketRepositories;

namespace BitBucket
{
    public class BitBucketProjectHandler(string token, BitBucketStorage<Project> dataset) : BitBucket<BitBucketStorage<Project>>(token, dataset), iBitBucketHandler
    {
        public async Task GetAllAsync(string key = "0")
        {
            int? start = 0;
            bool? isLastPage = false;

            var projects = new List<Project>();


            using (HttpClient client = new HttpClient())
            {
                // Add Authorization header
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", HTTPaccesstoken);
                var listofRepos = new List<string>();
                try
                {
                    while (isLastPage == false)
                    {
                        // Send GET request
                        var response = await client.GetAsync($"{_baseURL}/projects?start={start}{limitposURL}");

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
                            CheckAndThrowIfError(await response.Content.ReadAsStringAsync());
                        }
                    }
                }
                catch (Exception ex)
                {
                    CatchAndPromptErr(ex);
                }
            }
            lock (_dataset)
            {
                _dataset.Update(projects, key);
            }
        }
    }
}
