using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.ViewModels
{
    public class ProductVariantViewModel
    {
        public int Id { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal Priced { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public int ColorId { get; set; }
        public string? ColorName { get; set; }
        public string? ColorIconName { get; set; }
        public string? ColorHexCode { get; set; }
        public string? CoverImageName { get; set; }
        public IList<string> ImageNames { get; set; } = [];
    }

    public class ProductVariantCreateViewModel
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public IFormFile? CoverImageFile { get; set; }
        public string? CoverImageName { get; set; }
        public List<IFormFile> ImageFiles { get; set; } = [];
        public List<ProductImage> Images { get; set; } = [];
        public int ColorId { get; set; }
        public List<SelectListItem> ColorSelectListItems { get; set; } = [];
        public int ProductId { get; set; }
        public List<SelectListItem> ProductSelectListItems { get; set; } = [];

    }
    public class ProductVariantUpdateViewModel
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public IFormFile? CoverImageFile { get; set; }
        public string? CoverImageName { get; set; }
        public List<IFormFile> ImageFiles { get; set; } = [];
        public List<ProductImage> ProductImages { get; set; } = [];
        public int ColorId { get; set; }
        public List<SelectListItem> ColorSelectListItems { get; set; } = [];
        public int ProductId { get; set; }
        public List<SelectListItem> ProductSelectListItems { get; set; } = [];
    }
}
