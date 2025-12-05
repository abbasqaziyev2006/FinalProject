using System.Collections.Generic;

namespace EcommerceCoza.BLL.ViewModels
{
    public class HomeViewModel
    {
        public List<CategoryViewModel> FeaturedCategories { get; set; } = [];
        public List<ProductViewModel> FeaturedProducts { get; set; } = [];
        public List<ProductViewModel> HotDeals { get; set; } = [];
        public List<ProductViewModel> NewArrivals { get; set; } = [];
        public List<ProductViewModel> BestSellers { get; set; } = [];
    }
}