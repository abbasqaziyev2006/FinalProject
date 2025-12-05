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

        public HomeManager(ICategoryService categoryService, IProductService productService)
        {
            _categoryService = categoryService;
            _productService = productService;
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
                // Top 8 categories for carousel
                FeaturedCategories = categories.Take(8).ToList(),

                // Featured products - first 8 products
                FeaturedProducts = productsList.Take(8).ToList(),

                // Hot Deals - products with price less than 100
                HotDeals = productsList
                    .Where(p => p.BasePrice < 100 && p.ProductVariants.Any(v => v.Quantity > 0))
                    .Take(5)
                    .ToList(),

                // New Arrivals - latest 8 products
                NewArrivals = productsList
                    .OrderByDescending(p => p.Id)
                    .Take(8)
                    .ToList(),

                // Best Sellers - you can modify this logic based on your sales data
                BestSellers = productsList
                    .Where(p => p.ProductVariants.Any(v => v.Quantity > 0))
                    .Take(8)
                    .ToList()
            };

            return homeViewModel;
        }
    }
}