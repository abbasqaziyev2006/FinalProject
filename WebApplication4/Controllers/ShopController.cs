using EcommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Controllers
{
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;
        private readonly ILogger<ShopController> _logger;
        private const int PageSize = 60;

        public ShopController(IShopService shopService, ILogger<ShopController> logger)
        {
            _shopService = shopService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _shopService.GetShopViewModelAsync();
            // Order products so newest items show first, then take the first page
            var orderedProducts = model.Products.OrderByDescending(p => p.Id).ToList();
            var firstPageProducts = orderedProducts.Take(PageSize).ToList();
            model.Products = firstPageProducts;
            ViewBag.ProductCount = firstPageProducts.Count;
            ViewBag.TotalProducts = orderedProducts.Count;
            ViewBag.PageSize = PageSize;

            // IMPORTANT: expose the full ordered product list for accurate counts/filters
            ViewBag.AllProducts = orderedProducts;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreProducts(int skip = 0, int take = PageSize)
        {
            // Get full product list and apply same ordering as Index
            var fullModel = await _shopService.GetShopViewModelAsync();
            var ordered = fullModel.Products.OrderByDescending(p => p.Id).ToList();
            var products = ordered.Skip(skip).Take(take).ToList();

            if (!products.Any())
            {
                return Json(new { hasMore = false, products = new List<object>(), total = ordered.Count });
            }

            var productData = products.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                detailsUrl = p.DetailsUrl,
                basePrice = p.BasePrice,
                categoryId = p.CategoryId,
                categoryName = p.Category?.Name,
                brandId = p.BrandId,
                brandName = p.Brand?.Name,
                firstVariant = p.ProductVariants.FirstOrDefault() != null ? new
                {
                    id = p.ProductVariants.First().Id,
                    coverImageName = p.ProductVariants.First().CoverImageName,
                    imageNames = p.ProductVariants.First().ImageNames,
                    quantity = p.ProductVariants.First().Quantity,
                    colorId = p.ProductVariants.First().ColorId,
                    colorName = p.ProductVariants.First().ColorName,
                    colorHexCode = p.ProductVariants.First().ColorHexCode,
                    size = p.ProductVariants.First().Size
                } : null,
                variants = p.ProductVariants.Select(v => new
                {
                    id = v.Id,
                    colorId = v.ColorId,
                    colorName = v.ColorName,
                    colorHexCode = v.ColorHexCode,
                    size = v.Size
                }).ToList(),
                colorIds = string.Join(",", p.ProductVariants.Select(v => v.ColorId).Where(id => id > 0).Distinct()),
                sizes = string.Join(",", p.ProductVariants.Where(v => !string.IsNullOrEmpty(v.Size)).Select(v => v.Size).Distinct()),
                rating = p.Rating,
            }).ToList();

            var hasMore = (skip + products.Count) < ordered.Count;
            return Json(new { hasMore, products = productData, total = ordered.Count });
        }


        [HttpGet]
        public async Task<IActionResult> SearchProducts(string query)
        {
            try
            {
                _logger.LogInformation($"Search request received: '{query}'");

                // Validate query (return empty results, HTTP 200)
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    _logger.LogWarning("Search query too short or empty");
                    return Ok(new { products = new List<object>() });
                }

                var model = await _shopService.GetShopViewModelAsync();

                var searchResults = model.Products
                    .Where(p =>
                        (p.Name != null && p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Category?.Name != null && p.Category.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                        (p.Brand?.Name != null && p.Brand.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    )
                    .Take(10)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        basePrice = p.BasePrice,
                        coverImageName = p.ProductVariants.FirstOrDefault()?.CoverImageName ?? "no-image.png",
                        categoryName = p.Category?.Name ?? "Uncategorized",
                        brandName = p.Brand?.Name,
                        detailsUrl = p.DetailsUrl,
                        rating = p.Rating
                    })
                    .ToList();

                _logger.LogInformation($"Found {searchResults.Count} products for query: '{query}'");

                return Ok(new { products = searchResults });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching products with query: '{query}'");
                // Return 500 with JSON payload so client can show a proper error
                return StatusCode(500, new { products = new List<object>(), error = ex.Message });
            }
        }
    }
}