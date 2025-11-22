using System.ComponentModel.DataAnnotations;

namespace EcommerceCoza.MVC.Models
{
    public class RegisterViewModel
    {
        public required string UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        [DataType(DataType.EmailAddress)]
        public required string Email { get; set; }
        [DataType(DataType.Password)]
        public required string Password { get; set; }
    }
}
