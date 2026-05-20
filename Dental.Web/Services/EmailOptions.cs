namespace Dental.Web.Services;

public class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; set; }
    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public bool UseSsl { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string FromAddress { get; set; } = "noreply@dental.local";
    public string FromName { get; set; } = "Dental Clinic";
    public string FrontendBaseUrl { get; set; } = "http://localhost:4300";
}
