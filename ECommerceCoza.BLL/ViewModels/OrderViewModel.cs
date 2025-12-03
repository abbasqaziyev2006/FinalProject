using ECommerceCoza.DAL.DataContext.Entities;
using System.ComponentModel.DataAnnotations;

namespace EcommerceCoza.BLL.ViewModels
{
    public class OrderViewModel
    {
        public int Id { get; set; }
        public string? Discount { get; set; }
        public string? AppUserId { get; set; }
        public string? AppUserName { get; set; }
        public List<OrderDetailViewModel> OrderDetails { get; set; } = [];
        public bool GiftWrap { get; set; }
        public string? Note { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; } = null!;

        public decimal TotalPrice { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal EndPrice { get; set; }
        public int TotalCount { get; set; }

        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? DeliveredDate { get; set; } 
        public DateTime? CanceledDate { get; set; } 

        public int AddressId { get; set; }
        public AddressViewModel? Address { get; set; }

        // ✅ Hesaplanan Özellikler
        public decimal Tax => TotalPrice * 0.21m; // %21 KDV (isteğe göre ayarlayın)
        public string Phone => Address?.Phone ?? "N/A";
        public string PostalCode => Address?.PostalCode ?? "N/A";
        public string FormattedCreatedAt => CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "N/A";
        public string FormattedDeliveredDate => DeliveredDate?.ToString("yyyy-MM-dd") ?? "";
        public string FormattedCanceledDate => CanceledDate?.ToString("yyyy-MM-dd") ?? "";
    }

    public class OrderCreateViewModel
    {
        public bool HasAppliedDiscount { get; set; }
        public string? Discount { get; set; }
        public int? DiscountCodeId { get; set; }
        public string? AppUserId { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal EndPrice { get; set; }
        public List<OrderDetailCreateViewModel> OrderDetails { get; set; } = [];
        public bool GiftWrap { get; set; }
        public string? Note { get; set; }
        public string Email { get; set; } = null!;
        public OrderStatus OrderStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public AddressCreateViewModel? AddressCreateViewModel { get; set; }
        public AddressViewModel? AddressViewModel { get; set; }
        public bool AcceptTermsConditions { get; set; }
        public decimal TotalPrice { get; set; }
        public BasketViewModel? BasketViewModel { get; set; }
    }

    public class OrderUpdateViewModel
    {
        public int Id { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime? DeliveredDate { get; set; }
        public DateTime? CanceledDate { get; set; }
    }
}