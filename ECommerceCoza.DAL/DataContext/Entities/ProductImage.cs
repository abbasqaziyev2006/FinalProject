namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class ProductImage : TimeStample
    {
        public string ImageName { get; set; } = null!;
        public int ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

    }

 
}
