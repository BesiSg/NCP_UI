using BitBucketHandler;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Storage;

namespace BitBucket
{
    public class BitBucketProjectHandler(string token, StorageDat<Project> dataset, HttpClient client) : BitBucket<StorageDat<Project>>(token, dataset,client), iBitBucketHandler
    {
        public async Task<ErrorResult> GetAllAsync(string key = "0")
        {
            ClearErrorFlags();
            var projects = await _bitBucket.GetProjects();
            if (NoError() && _dataset != null)
            {
                lock (_dataset)
                {
                    _dataset?.Update(projects, key);
                }
            }
            return Result;
        }
    }
}
