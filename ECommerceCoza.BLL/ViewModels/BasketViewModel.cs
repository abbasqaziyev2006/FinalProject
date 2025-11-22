using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.ViewModels
{
    public class BasketViewModel
    {
        public List<BasketItemViewModel> Items { get; set; } = [];
        public decimal TotalPrice => Items.Sum(item => item.Price * item.Quantity);
        public int TotalCount => Items.Sum(item => item.Quantity);
    }

    public class BasketItemViewModel
    {
        public int ProductVariantId { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ColorName { get; set; } = null!;
        public string ImageName { get; set; } = null!;
        public string ProductName { get; set; } = null!;
        public decimal TotalPrice => Price * Quantity;
    }

    public class BasketCookieItemViewModel
    {
        public int ProductVariantId { get; set; }
        public int Quantity { get; set; }
    }
}
