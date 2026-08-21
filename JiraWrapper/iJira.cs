using Atlassian.Jira;
using RestSharp;
using RestSharp.Authenticators;
using Utility;

namespace JiraWrapper
{
    public interface iJIRA
    {
        IEnumerable<Issue> GetIssues(Project project, DateTime changeDate);
        IEnumerable<Comment> GetComments(string ticket);
        IEnumerable<Comment> GetComments(Issue issue);
        Issue GetTSGSDviaSDR(Issue issue);
        public string GetBeautifulSoupComments(Issue issue)
        {
            var summary = issue.Description.GetBeautifulSoup();
            var listofcomments = GetComments(issue);
            foreach (var comment in listofcomments)
            {
                var orga = comment.AuthorUser.DisplayName.Contains('@') ? comment.AuthorUser.DisplayName.Split('@')[0] : "Besi";
                var text = comment.Body.GetBeautifulSoup();
                summary += $"\n-----\nComment from {comment.AuthorUser.DisplayName} of {orga} at {comment.CreatedDate}: {text}";
            }
            return summary;
        }
    }
    public enum Project
    {
        AMSX,
        FSL,
        NCP,
        XHD,
        All,
    }

    public class PATTokenAuthentication : IAuthenticator
    {
        private readonly string _token;

        public PATTokenAuthentication(string token)
        {
            _token = token;
        }

        public ValueTask Authenticate(IRestClient client, RestRequest request, CancellationToken cancellationToken = default)
        {
            request.AddOrUpdateHeader(
                "Authorization",
                $"Bearer {_token}");

            return ValueTask.CompletedTask;
        }
    }
}
