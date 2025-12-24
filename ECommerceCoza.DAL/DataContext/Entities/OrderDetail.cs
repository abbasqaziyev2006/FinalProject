namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class OrderDetail : Entity
    {
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public int ProductVariantId { get; set; }
        public ProductVariant ProductVariant { get; set; } = null!;
    }
}
