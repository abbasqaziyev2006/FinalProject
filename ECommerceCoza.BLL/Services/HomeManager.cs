using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.Services
{
    public class HomeManager : IHomeService
    {
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ISliderService _sliderService;

        public HomeManager(ICategoryService categoryService, IProductService productService, ISliderService sliderService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _sliderService = sliderService;
        }

        public async Task<HomeViewModel> GetHomeViewModelAsync()
        {
            // Get active categories
            var categories = await _categoryService.GetAllAsync(
                predicate: x => !x.IsDeleted,
                orderBy: x => x.OrderBy(c => c.Id)
            );

            // Get all products with their related data
            var products = await _productService.GetAllAsync(
                predicate: x => !x.IsDeleted,
                include: x => x
                    .Include(pv => pv.ProductVariants).ThenInclude(i => i.ProductImages)
                    .Include(pv => pv.ProductVariants).ThenInclude(c => c.Color!)
                    .Include(c => c.Category)
                    .Include(b => b.Brand)
            );

            var productsList = products.ToList();

            var homeViewModel = new HomeViewModel
            {
                FeaturedCategories = categories.Take(8).ToList(),
                FeaturedProducts = productsList.Take(8).ToList(),
                HotDeals = productsList
                    .Where(p => p.ProductVariants.Any(v =>
                        v.SalePrice.HasValue &&
                        v.SalePrice.Value < v.Price &&
                        v.Quantity > 0
                    ))
                    .OrderByDescending(p => {
                        var bestDiscount = p.ProductVariants
                            .Where(v => v.SalePrice.HasValue && v.SalePrice.Value < v.Price && v.Quantity > 0)
                            .Select(v => (v.Price - v.SalePrice.Value) / v.Price * 100)
                            .DefaultIfEmpty(0)
                            .Max();
                        return bestDiscount;
                    })
                    .Take(5)
                    .ToList(),
                NewArrivals = productsList.OrderByDescending(p => p.Id).Take(8).ToList(),
            };

            // Load active sliders (new)
            var sliders = await _sliderService.GetAllAsync(
                predicate: s => !s.IsDeleted && s.IsActive,
                orderBy: q => q.OrderByDescending(s => s.Id)
            );
            homeViewModel.Sliders = sliders.ToList();

            return homeViewModel;
        }
    }
}