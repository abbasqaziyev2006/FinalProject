namespace EcommerceCoza.BLL.ViewModels
{
    public class OrderDetailViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductVariantId { get; set; }
        public ProductVariantViewModel? ProductVariant { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class OrderDetailCreateViewModel
    {
        public int Quantity { get; set; }
        public int OrderId { get; set; }
        public int ProductVariantId { get; set; }
        public ProductVariantViewModel ProductVariantViewModel { get; set; } = null!;
    }

    public class OrderDetailUpdateViewModel
    {

    }
}
