namespace Dental.Web.Services.OpenDental;

public sealed class OpenDentalOptions
{
    public const string SectionName = "OpenDental";

    /// <summary>
    /// When false, OpenDental proxy endpoints return 503 without calling the remote API.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Remote API root, default https://api.opendental.com/api/v1
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.opendental.com/api/v1";

    public string DeveloperKey { get; set; } = "";

    public string CustomerKey { get; set; } = "";
}
