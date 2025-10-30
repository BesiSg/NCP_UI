using BitBucketHandler;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Utility.Lib.BitBucketRepositories;

namespace BitBucket
{
    public class BitBucketBranchHandler(string token, BitBucketStorage<Branch> dataset) : BitBucket<BitBucketStorage<Branch>>(token, dataset), iBitBucketHandler
    {
        public async Task GetAllAsync(string key)//string projectKey, string repoSlug
        {
            var allBranches = new List<Branch>();
            int start = 0;
            bool isLastPage = false;
            var text = key.Split(' ');
            if (text.Length != 2) return;
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                while (!isLastPage)
                {
                    string url = $"{_baseURL}/projects/{text[0]}/repos/{text[1]}/branches?start={start}";

                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to fetch branches. Status code: {response.StatusCode}");
                        string error = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Error response: " + error);
                        break;
                    }

                    string json = await response.Content.ReadAsStringAsync();
                    var branchResponse = JsonSerializer.Deserialize<BranchResponse>(json);

                    allBranches.AddRange(branchResponse.values);

                    isLastPage = branchResponse.isLastPage;
                    start = branchResponse.start + branchResponse.size;
                }
            }
            lock (_dataset)
            {
                _dataset.Update(allBranches, $"{text[0]} {text[1]}");
            }
        }
    }
}
