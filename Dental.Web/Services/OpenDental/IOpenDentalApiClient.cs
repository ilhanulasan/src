namespace Dental.Web.Services.OpenDental;

public interface IOpenDentalApiClient
{
    Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string relativePathAndQuery,
        HttpContent? content,
        CancellationToken cancellationToken);
}
