

using System.Net;
using System.Net.Mail;

namespace TheLightStore.Services.Email;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var smtpServer = _config["EmailSettings:SmtpServer"];
        var port = int.Parse(_config["EmailSettings:Port"]);
        var senderEmail = _config["EmailSettings:SenderEmail"];
        var username = _config["EmailSettings:Username"];
        var password = _config["EmailSettings:Password"];

        using var client = new SmtpClient(smtpServer, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage(senderEmail, toEmail, subject, body)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mailMessage);
        _logger.LogInformation("Email sent to {ToEmail}", toEmail);
    }
}
