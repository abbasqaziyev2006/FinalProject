using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommerceCoza.BLL.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }
        public string DetailsUrl => $"{Name?.Replace(" ", "-").Replace("/", "-")}-{Id}";
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? AdditionalInformation { get; set; }
        public decimal BasePrice { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public string? CategoryName { get; set; }
        public int BrandId { get; set; }
        public Brand? Brand { get; set; }
        public string? BrandName { get; set; }
        public List<ProductVariantViewModel> ProductVariants { get; set; } = [];
        public bool IsInWishlist { get; set; }
        public List<int> WishlistItemIds { get; set; } = [];
        public int Rating { get; set; }
        public int ReviewCount { get; set; }
    }

    public class ProductCreateViewModel
    {
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? AdditionalInformation { get; set; }
        public decimal BasePrice { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public List<SelectListItem> CategorySelectListItems { get; set; } = [];
        public List<SelectListItem> BrandSelectListItems { get; set; } = [];
    }

    public class ProductUpdateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? AdditionalInformation { get; set; }
        public decimal BasePrice { get; set; }
        public int CategoryId { get; set; }
        public int BrandId { get; set; }
        public List<SelectListItem> CategorySelectListItems { get; set; } = [];
        public List<SelectListItem> BrandSelectListItems { get; set; } = [];
    }


}