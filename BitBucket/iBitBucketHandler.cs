using Utility;

namespace BitBucketHandler
{
    public interface iBitBucketHandler
    {
        Task<ErrorResult> GetAllAsync(string key);
    }

}
