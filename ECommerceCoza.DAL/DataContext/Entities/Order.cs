namespace ECommerceCoza.DAL.DataContext.Entities
{
    public class Order : TimeStample
    {
        public int? DiscountCodeId { get; set; }
        public DiscountCode? DiscountCode { get; set; }
        public string? AppUserId { get; set; }
        public AppUser? AppUser { get; set; }
        public List<OrderDetail> OrderDetails { get; set; } = [];
        public bool GiftWrap { get; set; }
        public string? Note { get; set; }
        public string Email { get; set; } = null!;
        public decimal DiscountAmount { get; set; }
        public decimal EndPrice { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public int AddressId { get; set; }
        public Address Address { get; set; } = null!;
        public decimal TotalPrice { get; set; }
    }

    public enum PaymentMethod
    {
        DirectBankTransfer,
        CashOnDelivery
    }

    public enum OrderStatus
    {
        InProgress,
        OnHold,
        Cancelled,
        Completed
    }
}
