using BitBucket;
using Git;
using System.Net.Http;
using System.Text.RegularExpressions;
using Utility;
using Utility.Lib.BitBucketRepositories;

namespace NCPBitbucket
{
    public class NCPBitbucketHandler : BitBucketRestAPI
    {
        protected const string _baseURL = "http://bb.eu.besi.corp/rest/api/1.0";
        protected const string _limitposURL = "&limit=1000";

        private readonly GitHandler _gitHandler;

        public NCPBitbucketHandler(string token, HttpClient client)
            : base(_baseURL, _limitposURL, token, client)
        {
            _gitHandler = new GitHandler("bb.eu.besi.corp:7999");
        }

        public Task<List<Branch>> GetBranchesOnRepository(
            string reposlug,
            CancellationToken cancellationToken = default)
        {
            return GetBranchesOnRepository(
                "NCP",
                reposlug,
                cancellationToken);
        }

        public Task<Dictionary<string, Tag>> GetTagsOnRepository(
            string reposlug,
            CancellationToken cancellationToken = default)
        {
            return GetTags(
                "NCP",
                reposlug,
                cancellationToken);
        }

        public async Task<bool> TagExist(
            string repository,
            string tagName,
            CancellationToken cancellationToken)
        {
            var tags = await GetTags(
                "NCP",
                repository,
                cancellationToken);

            return tags.Values.Any(t =>
                string.Equals(
                    t.displayId,
                    tagName,
                    StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<string> Clone(
            string project,
            string tag,
            Repository repository,
            string destinationPath,
            bool singlebranch = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var match = Regex.Match(
                    tag,
                    @"^(?<Version>[RV]\d+\.\d+\.\d+[A-Z]?)(?:\.(?<Customer>[A-Za-z_-]+?)(?<Revision>\d+)|\.(?<Patch>patch))?$",
                    RegexOptions.IgnoreCase);

                if (!match.Success)
                    return $"Fail to find matching in Tag {tag}";

                var version = match.Groups["Version"].Value.ToLowerInvariant();

                var customer = match.Groups["Customer"].Value
                    .ToLowerInvariant()
                    .Replace('_', '-');

                if (string.IsNullOrWhiteSpace(customer))
                    customer = "common";

                var branch = $"support/{customer}/{version}";

                cancellationToken.ThrowIfCancellationRequested();

                var repoSlug = repository.ToString();

                var tags = await GetTags(
                    "ncp",
                    repoSlug,
                    cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                if (!tags.ContainsKey(tag))
                {
                    return await _gitHandler.Clone(
                        project,
                        repoSlug,
                        branch,
                        destinationPath,
                        true,
                        cancellationToken);
                }

                return await _gitHandler.Clone(
                    project,
                    repoSlug,
                    branch,
                    tag,
                    destinationPath,
                    true,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> Clone(
            string project,
            string tag,
            string repository,
            string destinationPath,
            bool singlebranch = true,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!repository.TryToEnumByAttribute<Repository>(out var repo))
                    return "Fail to parse string to Repository enum";

                return await Clone(
                    project,
                    tag,
                    repo,
                    destinationPath,
                    singlebranch,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> GetReleaseOverAllContent(string commit, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                return await GetFileContent("NCP", "release_overall", "Release.cfg", commit, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public async Task<bool> BranchExist(string repository, string customer, string version, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var branch = $"support/{customer.ToLowerInvariant()}/{version.ToLowerInvariant()}";
                return await _gitHandler.BranchExists("NCP", repository, branch, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public async Task<bool> BranchExist(string repository, string branch, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await _gitHandler.BranchExists("NCP", repository, branch, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }

    public enum Repository
    {
        [NameAttribute("COMH")]
        units_comh,

        [NameAttribute("MAHA")]
        units_maha,

        [NameAttribute("PR3N")]
        units_pr3n,

        [NameAttribute("SEMW")]
        units_semw,

        [NameAttribute("BOFO")]
        units_bofo,

        [NameAttribute("HPDG")]
        units_hpdg,

        [NameAttribute("LFOL")]
        units_lfol,

        [NameAttribute("LPLO")]
        units_lplo,

        [NameAttribute("PPWA")]
        units_ppwa,

        [NameAttribute("PR2N")]
        units_pr2n,

        [NameAttribute("PR4N")]
        units_pr4n,

        [NameAttribute("SEMA")]
        units_sema,

        [NameAttribute("SEMO")]
        units_semo,

        [NameAttribute("SPR2")]
        units_spr2,

        [NameAttribute("Version_MMI")]
        mmi,
    }
}