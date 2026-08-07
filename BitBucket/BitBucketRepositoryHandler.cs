using BitBucketHandler;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Storage;

namespace BitBucket
{
    public class BitBucketRepositoryHandler(string token, StorageDat<Repository> dataset, HttpClient client) : BitBucket<StorageDat<Repository>>(token, dataset, client), iBitBucketHandler
    {
        public async Task<ErrorResult> GetAllAsync(string key)//string projectKey
        {
            ClearErrorFlags();
            var listofRepos = await _bitBucket.GetRepositoriesOnProject(key);
            if (NoError())
            {
                lock (_dataset)
                {
                    _dataset.Update(listofRepos, key);
                }
            }
            return Result;
        }
    }
}
