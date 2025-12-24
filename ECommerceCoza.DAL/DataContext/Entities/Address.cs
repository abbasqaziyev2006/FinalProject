namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class Address : TimeStample
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Company { get; set; } = null!;
        public string Country { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Adress { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string PostalCode { get; set; } = null!;
        public bool IsDefault { get; set; }
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
        public List<Order> Orders { get; set; } = [];
    }
}
