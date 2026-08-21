
using RestSharp;
using System.Text.Json;
using Utility.Lib.AtlassianService;

namespace WikiWrapper
{
    public class AtlassianWiki
    {
        private readonly IAtlassianService _client;
        public AtlassianWiki(IAtlassianService atlassianService)
        {
            _client = atlassianService;
        }
        public async Task<string> GetPageContentAsync(
             long pageId,
             CancellationToken cancellationToken = default)
        {
            var request = new RestRequest(
                $"/wiki/rest/api/content/{pageId}");

            request.AddQueryParameter(
                "expand",
                "body.storage");

            var response = await _client.ExecuteAsync(
                request,
                cancellationToken);

            if (!response.IsSuccessful)
            {
                throw new Exception(response.Content);
            }

            using var doc = JsonDocument.Parse(response.Content!);

            return doc.RootElement
                .GetProperty("body")
                .GetProperty("storage")
                .GetProperty("value")
                .GetString() ?? string.Empty;
        }

    }
}
