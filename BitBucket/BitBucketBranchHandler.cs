using BitBucketHandler;
using System.Net.Http;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Storage;

namespace BitBucket
{
    public class BitBucketBranchHandler(string token, StorageDatList<Branch> dataset, HttpClient client) : BitBucket<StorageDatList<Branch>>(token, dataset, client), iBitBucketHandler
    {
        public async Task<ErrorResult> GetAllAsync(string key)//string projectKey, string repoSlug
        {
            ClearErrorFlags();
            var allBranches = new List<Branch>();
            var text = key.Split(' ');
            try
            {
                if (text.Length != 2) CheckAndThrowIfError($"Invalid key length !=2, key length provided is {text.Length}");
                allBranches = await _bitBucket.GetBranchesOnRepository(text[0], text[1]);
            }
            catch (Exception ex)
            {
                CatchAndPromptErr(ex);
            }
            if (NoError())
            {
                lock (_dataset)
                {
                    _dataset.Replace(allBranches, $"{text[0]} {text[1]}");
                }
            }
            return Result;
        }
    }
}
