using BitBucketHandler;
using System.Net.Http;
using System.Text.RegularExpressions;
using Utility;
using Utility.Lib.BitBucketRepositories;
using Utility.Lib.Storage;

namespace BitBucket
{
    //this uses a different strategy than BitBucketCommitHandler. This seaches for all Tags in the repository instead of by branches in the repository.
    public class BitBucketTagHandler(string token, StorageDatList<Tag> dataset, HttpClient client) : BitBucket<StorageDatList<Tag>>(token, dataset, client), iBitBucketHandler
    {
        public (Tag, string) GetLatestTagAndNext(string projectkey, string reposlug)
        {
            var key = $"{projectkey} {reposlug}";
            if (!_dataset.Storage.ContainsKey(key)) return (null, string.Empty);
            var latest = _dataset.Storage[key].FirstOrDefault(tag => tag.displayId != string.Empty && tag.displayId != null && tag.latestCommit != string.Empty && tag.latestCommit != null);
            if (latest == null) return (null, string.Empty);
            return (latest, GetNextTag(latest.displayId));
        }
        private string GetNextTag(string latestTag)
        {
            if (latestTag == null) return string.Empty;
            var parts = latestTag.Split('.');
            if (parts.Length != 4) return string.Empty;
            // Match consecutive digits
            Match digitsMatch = Regex.Match(latestTag, @"\d+$");
            string digits = digitsMatch.Success ? digitsMatch.Value : string.Empty;
            var tag = latestTag.Remove(latestTag.Count() - digits.Length);
            if (digits == string.Empty) return string.Empty;
            var nextrev = int.Parse(digits) + 1;
            return tag + nextrev.ToString();
        }
        public async Task<ErrorResult> GetAllAsync(string key)//$"{project.key} {repository.slug}"
        {
            ClearErrorFlags();
            var text = key.Split(' ');
            try
            {
                if (text.Length != 2) CheckAndThrowIfError($"Expected Key length =2, actual key length {text.Length}");
            }
            catch (Exception e) { CatchAndPromptErr(e); }
            if (NoError())
            {
                var projectkey = text[0];
                var reposlug = text[1];
                var dictkey = $"{projectkey} {reposlug}";
                var tags = await _bitBucket.GetTags(projectkey, reposlug);
                var tagsOnBranch = new List<Tag>();
                lock (_dataset)
                {
                    _dataset.Replace(tags.Values, dictkey);
                }
            }
            return Result;
        }
    }
}
