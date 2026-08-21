using BitBucketHandler;
using System.Net.Http;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Storage;

namespace BitBucket
{
    public class BitBucketRepositoryHandler(string token, StorageDatList<Repository> dataset, HttpClient client) : BitBucket<StorageDatList<Repository>>(token, dataset, client), iBitBucketHandler
    {
        public async Task<ErrorResult> GetAllAsync(string key)//string projectKey
        {
            ClearErrorFlags();
            var listofRepos = await _bitBucket.GetRepositoriesOnProject(key);
            if (NoError())
            {
                lock (_dataset)
                {
                    _dataset.Replace(listofRepos, key);
                }
            }
            return Result;
        }
    }
}
