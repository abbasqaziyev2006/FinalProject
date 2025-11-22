using System.ComponentModel.DataAnnotations;

namespace EcommerceCoza.MVC.Models
{
    public class LoginViewModel
    {
        public required string Email { get; set; }
        [DataType(DataType.Password)]
        public required string Password { get; set; }
        public string? ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }
}
