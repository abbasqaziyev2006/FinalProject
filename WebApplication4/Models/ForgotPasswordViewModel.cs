using System.ComponentModel.DataAnnotations;

namespace EcommerceCoza.MVC.Models
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}