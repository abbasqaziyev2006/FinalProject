namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class DiscountCode : TimeStample
    {
        public string Code { get; set; } = null!;
        public List<Order> Orders { get; set; } = [];
        public bool IsActive { get; set; }
        public int SalePercentage { get; set; }
    }
}
