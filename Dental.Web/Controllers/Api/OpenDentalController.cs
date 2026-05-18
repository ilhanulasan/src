using Dental.Web.Services.OpenDental;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Dental.Web.Controllers.Api;

[ApiController]
[Route("api/opendental")]
public sealed class OpenDentalController(
    IOpenDentalApiClient apiClient,
    IOptions<OpenDentalOptions> options,
    ILogger<OpenDentalController> log) : ControllerBase
{
    /// <summary>
    /// Proxies GET https://api.opendental.com/api/v1/patients with the same query string (Limit, Offset, etc.).
    /// </summary>
    [HttpGet("patients")]
    public Task<IActionResult> GetPatients(CancellationToken ct) =>
        ProxyGetAsync($"patients{Request.QueryString}", ct);

    /// <summary>
    /// Proxies GET .../patients/{PatNum}.
    /// </summary>
    [HttpGet("patients/{patNum}")]
    public Task<IActionResult> GetPatient(string patNum, CancellationToken ct) =>
        ProxyGetAsync($"patients/{Uri.EscapeDataString(patNum)}{Request.QueryString}", ct);

    private async Task<IActionResult> ProxyGetAsync(string pathAndQuery, CancellationToken ct)
    {
        var opts = options.Value;
        if (!opts.Enabled)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { error = "OpenDental integration is disabled. Set OpenDental:Enabled to true." });
        }

        if (string.IsNullOrWhiteSpace(opts.DeveloperKey) || string.IsNullOrWhiteSpace(opts.CustomerKey))
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new { error = "OpenDental DeveloperKey and CustomerKey are not configured." });
        }

        try
        {
            using var response = await apiClient.SendAsync(HttpMethod.Get, pathAndQuery, content: null, ct)
                .ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            return new ContentResult
            {
                StatusCode = (int)response.StatusCode,
                Content = body,
                ContentType = response.Content.Headers.ContentType?.MediaType ?? "application/json",
            };
        }
        catch (HttpRequestException ex)
        {
            log.LogWarning(ex, "OpenDental API request failed for {Path}", pathAndQuery);
            return StatusCode(
                StatusCodes.Status502BadGateway,
                new { error = "OpenDental API request failed.", detail = ex.Message });
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            log.LogWarning(ex, "OpenDental API timed out for {Path}", pathAndQuery);
            return StatusCode(StatusCodes.Status504GatewayTimeout, new { error = "OpenDental API timed out." });
        }
    }
}
