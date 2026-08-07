
using BitBucket;
using Git;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Policy;
using System.Text.RegularExpressions;
using Utility;

namespace NCPBitbucket
{
    public class NCPBitbucketHandler:BaseUtility
    {
        protected const string _baseURL = "http://bb.eu.besi.corp/rest/api/1.0";
        protected const string _limitposURL = "&limit=1000";
        private BitBucketRestAPI _restAPI;
        private GitHandler _gitHandler;
        public NCPBitbucketHandler(string token, HttpClient client) {
            _restAPI = new BitBucketRestAPI(_baseURL, _limitposURL, token, client);
            _gitHandler = new GitHandler("bb.eu.besi.corp:7999");
        }
        public Task<string> Clone(
            string project,
            string tag,
            string repository,
            string destinationPath,
            bool singlebranch = true,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(()=>
            {
                var ret = string.Empty;
                try
                {
                    var match = Regex.Match(
                        tag,
                        @"^(?<Version>[RV]\d+\.\d+\.\d+[A-Z]?)(?:\.(?<Customer>[A-Za-z_-]+?)(?<Revision>\d+)|\.(?<Patch>patch))?$",
                        RegexOptions.IgnoreCase);
                    var version = string.Empty;
                    var customer = string.Empty;
                    var branch = string.Empty;
                    if (match.Success)
                    {
                        version = match.Groups[1].Value.ToLowerInvariant();
                        customer = match.Groups[2].Value.ToLowerInvariant().Replace('_', '-');
                        if (string.IsNullOrWhiteSpace(customer))
                            customer = "common";
                        branch = $"support/{customer}/{version}";
                    }
                    else
                        return $"Fail to find matching in Tag {tag}";
                    if (!repository.TryToEnumByAttribute<Repository>(out var repo)) return "Fail to parse string to Repository enum";
                    var tags = _restAPI.GetTags("ncp", repo.ToString()).Result;
                    if (!tags.ContainsKey(version.ToLower()))
                        ret = _gitHandler.Clone(project, repo.ToString(), branch, destinationPath, true, cancellationToken).Result;
                    else
                        ret = _gitHandler.Clone(project, repo.ToString(), branch, tag, destinationPath, true, cancellationToken).Result;
                }
                catch(Exception ex)
                {
                    ret = ex.Message;
                }
                return ret;
            });
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
