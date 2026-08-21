using BitBucketHandler;
using System.Net.Http;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Storage;

namespace BitBucket
{
    public class BitBucketProjectHandler(string token, StorageDatList<Project> dataset, HttpClient client) : BitBucket<StorageDatList<Project>>(token, dataset, client), iBitBucketHandler
    {
        public async Task<ErrorResult> GetAllAsync(string key = "0")
        {
            ClearErrorFlags();
            var projects = await _bitBucket.GetProjects();
            if (NoError() && _dataset != null)
            {
                lock (_dataset)
                {
                    _dataset?.Replace(projects, key);
                }
            }
            return Result;
        }
    }
}
