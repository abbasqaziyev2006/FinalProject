using EcommerceCoza.BLL.Services;
using EcommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Http;
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
        private readonly IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private const string AppliedDiscountCodeKey = "AppliedDiscountCode";

        public BasketController(BasketManager basketManager, IOrderService orderService, IHttpContextAccessor httpContextAccessor)
        {
            _basketManager = basketManager;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        [HttpPost]
        public async Task<IActionResult> Add(int productVariantId, int quantity)
        {
            try
            {

                var basket = await _basketManager.AddToBasketAsync(productVariantId, quantity);

                return Json(new
                {
                    success = true,
                    message = "Product added to basket successfully!",
                    totalCount = basket.TotalCount,
                    totalPrice = basket.TotalPrice
                });
            }
            catch
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to add product to basket. Please try again."
                });
            }
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

        [HttpPost]
        public IActionResult RemoveDiscount()
        {
            Session?.Remove(AppliedDiscountCodeKey);
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> ApplyDiscount(string discountCode)
        {
            if (string.IsNullOrWhiteSpace(discountCode))
            {
                return Json(new
                {
                    success = false,
                    message = "Please enter a discount code"
                });
            }

            var discount = await _orderService.GetDiscount(discountCode);

            if (discount == null)
            {
       
                Session?.Remove(AppliedDiscountCodeKey);
                return Json(new
                {
                    success = false,
                    message = "Invalid or expired discount code"
                });
            }


            Session?.SetString(AppliedDiscountCodeKey, discountCode);

            var basket = await _basketManager.GetBasketAsync();
            var originalTotal = basket.TotalPrice;
            var discountAmount = (originalTotal * discount.SalePercentage) / 100;
            var finalPrice = originalTotal - discountAmount;

            return Json(new
            {
                success = true,
                salePercentage = discount.SalePercentage,
                originalTotal = Math.Round(originalTotal, 2),
                discountAmount = Math.Round(discountAmount, 2),
                finalPrice = Math.Round(finalPrice, 2)
            });
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

            await viewResult.View.RenderAsync(viewContext);

            return writer.ToString();
        }
    }
}