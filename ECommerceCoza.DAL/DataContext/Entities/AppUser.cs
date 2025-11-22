using Microsoft.AspNetCore.Identity;

namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class AppUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<WishlistItem> WishlistItems { get; set; } = [];
        public List<Order> Orders { get; set; } = [];
        public List<Address> Addresses { get; set; } = [];
    }

 
}
