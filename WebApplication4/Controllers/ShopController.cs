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
    }
}