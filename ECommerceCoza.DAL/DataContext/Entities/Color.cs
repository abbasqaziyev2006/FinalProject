namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class Color : TimeStample
    {
        public string Name { get; set; } = null!;
        public string? IconName { get; set; }
        public string HexCode { get; set; } = null!;
        public List<ProductVariant> ProductVariants { get; set; } = [];
    }

 
}
