using System.Net;

namespace Pos.Stock.Client.Services;

public sealed class ApiClient(HttpClient httpClient)
{
    public async Task<HttpStatusCode> GetHealthAsync(CancellationToken cancellationToken = default)
    {
        using var response = await httpClient.GetAsync("health", cancellationToken);
        return response.StatusCode;
    }
}
