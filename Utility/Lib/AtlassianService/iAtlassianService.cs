using RestSharp;

namespace Utility.Lib.AtlassianService
{
    public interface IAtlassianService
    {
        Task<RestResponse> ExecuteAsync(
            RestRequest request,
            CancellationToken token = default);
        Task<RestResponse<T>> ExecuteAsync<T>(
            RestRequest request,
            CancellationToken token = default);
        void InitClient(string email, string apiToken);
    }
}
