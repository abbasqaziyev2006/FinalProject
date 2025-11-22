namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class WishlistItem : TimeStample
    {
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public string? AppUserId { get; set; }
        public AppUser AppUser { get; set; } = null!;
    }

 
}
