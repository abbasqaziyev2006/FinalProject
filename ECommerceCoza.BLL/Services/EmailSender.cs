using ECommerceCoza.BLL.Services.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> options, ILogger<EmailService> logger)
    {
        _emailSettings = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage, string senderRole = "User")
    {
        try
        {
            _logger.LogInformation($"Attempting to send email to {email} with subject: {subject}");

            using var client = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(
                    senderRole == "Admin" ? _emailSettings.AdminEmail : _emailSettings.UserEmail,
                    senderRole == "Admin" ? _emailSettings.AdminPassword : _emailSettings.UserPassword
                ),
                EnableSsl = true
            };

            var mailMessage = new MailMessage(
                senderRole == "Admin" ? _emailSettings.AdminEmail : _emailSettings.UserEmail,
                email,
                subject,
                htmlMessage
            )
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email successfully sent to {email}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {email}. Error: {ex.Message}");
            return false;
        }
    }
}