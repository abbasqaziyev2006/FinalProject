using EcommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Controllers
{
    public class ShopController : Controller
    {
        private readonly IShopService _shopService;

        public ShopController(IShopService shopService)
        {
            _shopService = shopService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _shopService.GetShopViewModelAsync();

            ViewBag.ProductCount = model.Products.Count;

            return View(model);
        }

        public async Task<IActionResult> Partial(int skip)
        {
            var model = await _shopService.GetShopViewModelAsync();
            var products = model.Products.Skip(skip).Take(4);

            return PartialView("_ProductCardPartial", products);
        }
    }
}
