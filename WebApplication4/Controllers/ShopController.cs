using EcommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Controllers
{
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;
        private const int PageSize = 12;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
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
    }
}