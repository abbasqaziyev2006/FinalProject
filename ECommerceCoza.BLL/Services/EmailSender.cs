using ECommerceCoza.BLL.Services.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _emailSettings = options.Value;
    }

    public async Task<bool> SendEmailAsync(string email, string subject, string htmlMessage, string senderRole = "User")
    {
        try
        {
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
            return true;
        }
        catch
        {
            return false;
        }
    }

}
