namespace ECommerceCoza.BLL.Services.Contracts
{

    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string email, string subject, string message, string senderRole = "User");
    }

}
