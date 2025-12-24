using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.BLL.Services
{
    public class ShopManager : IShopService
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly IBrandService _brandService;
        private readonly IColorService _colorService;

        public ShopManager(ICategoryService categoryService, IProductService productService, IBrandService brandService, IColorService colorService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _brandService = brandService;
            _colorService = colorService;
        }

        public async Task<ShopViewModel> GetShopViewModelAsync()
        {
            var categories = await _categoryService.GetAllAsync(predicate: x => !x.IsDeleted);
            var products = await _productService.GetAllAsync(
                predicate: x => !x.IsDeleted,
                include: x => x
                    .Include(pv => pv.ProductVariants).ThenInclude(i => i.ProductImages)
                    .Include(pv => pv.ProductVariants).ThenInclude(c => c.Color!)
                    .Include(p => p.Brand!)
                    .Include(p => p.Category!)
            );

            var brands = await _brandService.GetAllAsync(predicate: b => b.IsActive && !b.IsDeleted);
            var colors = await _colorService.GetAllAsync();

            var shopViewModel = new ShopViewModel
            {
                Categories = categories.ToList(),
                Products = products.ToList(),
                Brands = brands.ToList(),
                Colors = colors.ToList()
            };

            return shopViewModel;
        }
    }
}