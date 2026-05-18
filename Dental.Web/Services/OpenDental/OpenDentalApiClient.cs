using Microsoft.Extensions.Options;

namespace Dental.Web.Services.OpenDental;

public sealed class OpenDentalApiClient : IOpenDentalApiClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenDentalOptions _options;

    public OpenDentalApiClient(HttpClient httpClient, IOptions<OpenDentalOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePathAndQuery,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        var path = relativePathAndQuery.TrimStart('/');
        using var request = new HttpRequestMessage(method, path);
        request.Headers.TryAddWithoutValidation(
            "Authorization",
            $"ODFHIR {_options.DeveloperKey}/{_options.CustomerKey}");

        request.Headers.TryAddWithoutValidation("Accept", "application/json");
        request.Content = content;
        if (content is null)
        {
            request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        }

        return await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
    }
}
