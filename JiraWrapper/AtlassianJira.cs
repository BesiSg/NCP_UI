using RestSharp;
using System.IO;
using System.Text.Json;
using Utility;
using Utility.Lib.AtlassianService;
using Utility.Lib.Ticket;
using Utility.Lib.UserAccount;
using Comment = Utility.Lib.Ticket.Comment;
using Issue = Utility.Lib.Ticket.Issue;

namespace JiraWrapper
{
    public class AtlassianJira
    {
        private readonly IAtlassianService _client;
        public AtlassianJira(UserAccount account, IAtlassianService client)
        {
            _client = client;
        }
        public IEnumerable<Issue> GetIssues(Project project, DateTime changeDate)
        {
            var issues = new List<Issue>();
            var projectJql = project == Project.All ? "FSL, XHD, FCL, NCP, AMS-X" : project.ToString();
            if (changeDate == null || changeDate.CompareTo(new DateTime()) == 0) changeDate = new DateTime(2023, 06, 15);
            var changeDateJql = changeDate.ToString("yyyy-MM-dd");
            int startAt = 0;
            int total = 0;
            int issuesPerRequest = 100;
            var jql = $"project in ({projectJql}) AND issuetype = \"Service Desk Request\" AND \"Pkg Department\" in (Software,Motion,Vision, \"Process Technology\") AND (updatedDate > {changeDateJql}  OR createdDate > {changeDateJql})";
            var something = FetchAllIssuesAsync(jql).Result;
            issues.AddRange(something);
            return issues;
        }
        public Issue GetTSGSDviaSDR(Issue issue)
        {
            if (issue?.Fields?.IssueType?.Name != "Service Desk Request") return null;
            var linkedissues = issue.Fields.IssueLinks;
            foreach (var linkedissue in linkedissues)
            {
                if (linkedissue.Type.Name != "Problem/Incident") continue;
                if (linkedissue.OutwardIssue?.Key != null)
                {
                    if (issue.Fields.Summary.Contains(linkedissue.OutwardIssue.Key.ToString())) return GetIssueByTicketnumber(linkedissue.OutwardIssue.Key);
                }
                else if (linkedissue.InwardIssue?.Key != null)
                {
                    if (issue.Fields.Summary.Contains(linkedissue.InwardIssue?.Key?.ToString())) return GetIssueByTicketnumber(linkedissue.InwardIssue.Key);
                }

            }
            return null;
        }
        public Issue GetIssueByTicketnumber(string ticketnumber)
        {
            var jql = $"key = {ticketnumber}";
            return FetchAllIssuesAsync(jql).Result.FirstOrDefault();
        }


        public async Task<Issue?> GetIssueAsync(string ticketNumber)
        {
            var request = new RestRequest($"rest/api/3/issue/{ticketNumber}", Method.Get);

            request.AddParameter("fields", "*all");

            var response = await _client.ExecuteAsync<Issue>(request);

            if (!response.IsSuccessful)
            {
                throw new Exception(
                    $"Failed to retrieve issue {ticketNumber}: {response.StatusCode} {response.ErrorMessage}");
            }

            return response.Data;
        }
        public async Task GetPDFAttachment(string ticketNumber, string path)
        {
            var data = await GetIssueAsync(ticketNumber);

            foreach (var attachment in data.Fields.Attachment ?? [])
            {
                if (!attachment.Filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    continue;

                var request = new RestRequest(attachment.Content, Method.Get);

                var response = await _client.ExecuteAsync(request);

                if (response.IsSuccessful && response.RawBytes != null)
                {
                    var filePath = Path.Combine(path, attachment.Filename);
                    await File.WriteAllBytesAsync(filePath, response.RawBytes);
                }
            }
        }

        private async Task<List<Issue>> FetchAllIssuesAsync(string jql)
        {
            var allIssues = new List<Issue>();
            const int maxResultsPerRequest = 100;

            string nextPageToken = null;
            bool isLast = false;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            while (!isLast)
            {
                var request = new RestRequest("rest/api/3/search/jql", Method.Get);
                request.AddHeader("Accept", "application/json");

                request.AddParameter("jql", jql);
                request.AddParameter("maxResults", maxResultsPerRequest);
                request.AddParameter("fields", "*all");

                // ✅ This is the key for next page
                if (!string.IsNullOrEmpty(nextPageToken))
                {
                    request.AddParameter("nextPageToken", nextPageToken);
                }

                var response = await _client.ExecuteAsync(request);

                if (!response.IsSuccessful)
                {
                    throw new Exception($"Failed to fetch issues: {response.StatusCode} {response.Content}");
                }

                var jiraResult = JsonSerializer.Deserialize<JiraSearchResult>(response.Content, options);

                if (jiraResult?.Issues != null && jiraResult.Issues.Count > 0)
                    allIssues.AddRange(jiraResult.Issues);

                // ✅ Update loop variables from response
                isLast = jiraResult?.isLast ?? true;
                nextPageToken = jiraResult?.nextPageToken;

                // Safety: avoid infinite loop if Jira says not last but token missing
                if (!isLast && string.IsNullOrEmpty(nextPageToken))
                    throw new InvalidOperationException("Pagination error: isLast=false but nextPageToken is empty.");
            }

            return allIssues;
        }

        public string GetBeautifulSoupComments(Issue issue)
        {
            var summary = issue.Fields.Description.ToPlainText();
            var listofcomments = GetComments(issue);
            foreach (var comment in listofcomments)
            {
                var orga = comment.UpdateAuthor.DisplayName;
                var text = comment.Body.ToPlainText().GetBeautifulSoup();
                summary += $"\n-----\nComment from {comment.UpdateAuthor.DisplayName} of {orga} at {comment.Updated}: {text}";
            }
            return summary;
        }

        private IEnumerable<Comment> GetComments(Issue issue)
        {
            if (issue.Fields?.Comment?.Comments != null)
                return issue.Fields.Comment.Comments;
            return null;
        }
    }
}
