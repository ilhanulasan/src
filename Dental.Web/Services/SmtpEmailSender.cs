using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace Dental.Web.Services;

public class SmtpEmailSender(IOptions<EmailOptions> options, ILogger<SmtpEmailSender> log) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(toEmail))
        {
            log.LogWarning("Skipped email send: recipient is empty");
            return;
        }

        if (!_options.Enabled)
        {
            log.LogInformation(
                "Email disabled. Would send to {To} subject {Subject}\n{Body}",
                toEmail,
                subject,
                htmlBody);
            return;
        }

        using var message = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(toEmail.Trim());

        using var client = new SmtpClient(_options.Host, _options.Port)
        {
            EnableSsl = _options.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
        };

        if (!string.IsNullOrWhiteSpace(_options.UserName))
        {
            client.Credentials = new NetworkCredential(_options.UserName, _options.Password);
        }

        await client.SendMailAsync(message, ct);
        log.LogInformation("Sent email to {To} with subject {Subject}", toEmail, subject);
    }
}
