using System.ComponentModel.DataAnnotations;

namespace EcommerceCoza.BLL.ViewModels
{
    public class UserViewModel
    {
        public string Id { get; set; } = null!;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public int TotalOrders { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
    public class UserListViewModel
    {
        public List<UserViewModel> Users { get; set; } = [];
        public string? SearchTerm { get; set; }
    }

    public class EditUserViewModel
    {
        public string? Id { get; set; }

        [Display(Name = "Username")]
        public string? UserName { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Display(Name = "First Name")]
        public string? FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string? LastName { get; set; }

        [Phone]
        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        public List<string> AllRoles { get; set; } = new List<string>();
        public List<string> SelectedRoles { get; set; } = new List<string>();
    }
}
