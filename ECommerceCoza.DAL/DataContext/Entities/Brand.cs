namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class Brand : TimeStample
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Product> Products { get; set; } = [];
    }
}
