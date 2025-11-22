namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class ProductVariant : TimeStample
    {
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int ColorId { get; set; }
        public Color? Color { get; set; }
        public string CoverImageName { get; set; } = null!;
        public decimal Price { get; set; }
        public int SalePercentage { get; set; }
        public int Quantity { get; set; }
        public List<ProductImage> ProductImages { get; set; } = [];
        public List<OrderDetail> OrderDetails { get; set; } = [];
    }

 
}
