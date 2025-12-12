using EcommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceCoza.MVC.Controllers
{
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;
        private readonly ILogger<ShopController> _logger;
        private const int PageSize = 12;

        public ShopController(IShopService shopService, ILogger<ShopController> logger)
        {
            _shopService = shopService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _shopService.GetShopViewModelAsync();

            var firstPageProducts = model.Products.Take(PageSize).ToList();
            model.Products = firstPageProducts;

            ViewBag.ProductCount = firstPageProducts.Count;
            ViewBag.TotalProducts = (await _shopService.GetShopViewModelAsync()).Products.Count;
            ViewBag.PageSize = PageSize;

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> LoadMoreProducts(int skip = 0, int take = 12)
        {
            var model = await _shopService.GetShopViewModelAsync();
            var products = model.Products.Skip(skip).Take(take).ToList();

            if (!products.Any())
            {
                return Json(new { hasMore = false, products = new List<object>() });
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
                reviewCount = p.ReviewCount
            }).ToList();

            var totalProducts = model.Products.Count;
            var hasMore = (skip + take) < totalProducts;

            return Json(new { hasMore, products = productData });
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts([FromQuery] string query)
        {
            _logger.LogInformation($"=== SearchProducts STARTED === Query: '{query}'");

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                _logger.LogWarning("Query validation failed");
                return Json(new { products = Array.Empty<object>() });
            }

            try
            {
                var shopData = await _shopService.GetShopViewModelAsync();

                if (shopData?.Products == null)
                {
                    _logger.LogError("ShopData or Products is NULL!");
                    return Json(new { products = Array.Empty<object>() });
                }

                _logger.LogInformation($"Total products loaded: {shopData.Products.Count}");

                // Log first few products
                foreach (var p in shopData.Products.Take(3))
                {
                    _logger.LogInformation($"Product: '{p.Name}'");
                }

                var normalizedQuery = query.Trim().ToLowerInvariant();
                _logger.LogInformation($"Normalized query: '{normalizedQuery}'");

                var searchResults = shopData.Products
                    .Where(p => !string.IsNullOrWhiteSpace(p.Name) &&
                               p.Name.ToLowerInvariant().Contains(normalizedQuery))
                    .Take(10)
                    .Select(p => new
                    {
                        id = p.Id,
                        name = p.Name,
                        detailsUrl = p.DetailsUrl,
                        basePrice = p.BasePrice,
                        categoryName = p.Category?.Name ?? "Uncategorized",
                        brandName = p.Brand?.Name ?? "",
                        coverImageName = p.ProductVariants?.FirstOrDefault()?.CoverImageName ?? "product-placeholder.jpg"
                    })
                    .ToList();

                _logger.LogInformation($"=== SearchProducts COMPLETED === Found: {searchResults.Count} products");

                return Json(new { products = searchResults });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"=== SearchProducts ERROR === Query: '{query}'");
                return StatusCode(500, new
                {
                    products = Array.Empty<object>(),
                    error = "Search failed",
                    message = ex.Message
                });
            }
        }
    }
}