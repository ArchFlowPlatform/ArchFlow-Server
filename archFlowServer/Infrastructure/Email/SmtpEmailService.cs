using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace archFlowServer.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public SmtpEmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody)
    {
        var smtpHost = _configuration["Email:Smtp:Host"];
        var smtpPort = int.Parse(_configuration["Email:Smtp:Port"]!);
        var username = _configuration["Email:Smtp:Username"];
        var password = _configuration["Email:Smtp:Password"];
        var from = _configuration["Email:Smtp:From"];

        var client = new SmtpClient(smtpHost, smtpPort)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(from!),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(to);

        await client.SendMailAsync(message);
    }
}
