using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.BLL.Services
{
    public class ShopManager : IShopService
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;

        public ShopManager(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
        }

        public async Task<ShopViewModel> GetShopViewModelAsync()
        {
            var categories = await _categoryService.GetAllAsync(predicate: x => !x.IsDeleted);
            var products = await _productService.GetAllAsync(predicate: x => !x.IsDeleted
              , include: x => x
              .Include(pv => pv.ProductVariants).ThenInclude(i => i.ProductImages)
              .Include(pv => pv.ProductVariants).ThenInclude(c => c.Color!)
            );
   
            var shopViewModel = new ShopViewModel
            {
                Categories = categories.ToList(),
                Products = products.Take(4).ToList(),
            };

            return shopViewModel;
        }
    }
}
