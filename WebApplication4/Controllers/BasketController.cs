using EcommerceCoza.BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;

namespace EcommerceCoza.MVC.Controllers
{
    public class BasketController : Controller
    {
        private readonly BasketManager _basketManager;

        public BasketController(BasketManager basketManager)
        {
            _basketManager = basketManager;
        }

        [HttpPost]
        public IActionResult Add(int productVariantId, int quantity)
        {
            _basketManager.AddToBasket(productVariantId, quantity);

            return NoContent();
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            _basketManager.RemoveFromBasket(id);

            return NoContent();
        }

        public async Task<IActionResult> GetBasket()
        {
            var model = await _basketManager.GetBasketAsync();

            return Json(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeQuantity(int productVariantId, int change)
        {
            var basketViewModel = await _basketManager.ChangeQuantityAsync(productVariantId, change);

            return Json(new
            {
                success = true,
                basketViewModel
            });
        }

        [HttpPost]
        public async Task<IActionResult> ChangeQuantityC(int productVariantId, int change)
        {
            var basketViewModel = await _basketManager.ChangeQuantityAsync(productVariantId, change);
            var cartHtml = await RenderPartialViewToString("_CartPartialView", basketViewModel);

            return Json(new
            {
                success = true,
                basketViewModel,
                cartHtml
            });
        }

        public async Task<IActionResult> Index()
        {
            var model = await _basketManager.GetBasketAsync();
            return View(model);
        }

        private async Task<string> RenderPartialViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using var writer = new StringWriter();

            var viewEngine = HttpContext.RequestServices.GetService<ICompositeViewEngine>();
            var viewResult = viewEngine.FindView(ControllerContext, viewName, false);

            if (!viewResult.Success)
                throw new InvalidOperationException($"Could not found view {viewName}");

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempData,
                writer,
                new HtmlHelperOptions()
                );

            await viewResult.View.RenderAsync( viewContext );

            return writer.ToString();
        }
    }
}
