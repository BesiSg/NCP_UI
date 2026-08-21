using RestSharp;
using RestSharp.Authenticators;

namespace Utility.Lib.AtlassianService
{
    public class AtlassianService : IAtlassianService
    {
        private const string _host = "https://besi.atlassian.net";
        private RestClient _client;
        public AtlassianService()
        {
            _client = new RestClient();
        }
        public AtlassianService(
            string email,
            string apiToken)
        {
            _client = new RestClient(
                new RestClientOptions(_host)
                {
                    Authenticator =
                        new HttpBasicAuthenticator(
                            email,
                            apiToken)
                });
        }

        public Task<RestResponse> ExecuteAsync(
            RestRequest request,
            CancellationToken cancellationToken = default)
        {
            return _client.ExecuteAsync(
                request,
                cancellationToken);
        }

        public Task<RestResponse<T>> ExecuteAsync<T>(
            RestRequest request,
            CancellationToken cancellationToken = default)
        {
            return _client.ExecuteAsync<T>(
                request,
                cancellationToken);
        }

        public void InitClient(string email, string apiToken)
        {
            _client = new RestClient(
                new RestClientOptions(_host)
                {
                    Authenticator =
                        new HttpBasicAuthenticator(
                            email,
                            apiToken)
                });
        }
    }
}
