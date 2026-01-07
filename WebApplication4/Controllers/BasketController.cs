using EcommerceCoza.BLL.Services;
using EcommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Threading.Tasks;
using WebApplication4.Services;

namespace EcommerceCoza.MVC.Controllers
{
    public class BasketController : Controller
    {
        private readonly BasketManager _basketManager;
        private readonly IOrderService _orderService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrencyService _currencyService;
        private const string AppliedDiscountCodeKey = "AppliedDiscountCode";

        public BasketController(BasketManager basketManager, IOrderService orderService, IHttpContextAccessor httpContextAccessor, ICurrencyService currencyService)
        {
            _basketManager = basketManager;
            _orderService = orderService;
            _httpContextAccessor = httpContextAccessor;
            _currencyService = currencyService;
        }

        private ISession? Session => _httpContextAccessor.HttpContext?.Session;

        [HttpPost]
        public async Task<IActionResult> Add(int productVariantId, int quantity)
        {
            try
            {
                var basket = await _basketManager.AddToBasketAsync(productVariantId, quantity);

                object? discountInfo = null;
                var savedDiscountCode = Session?.GetString(AppliedDiscountCodeKey);
                if (!string.IsNullOrEmpty(savedDiscountCode))
                {
                    var discount = await _orderService.GetDiscount(savedDiscountCode);
                    if (discount != null)
                    {
                        var originalTotal = basket.TotalPrice;
                        var discountAmount = (originalTotal * discount.SalePercentage) / 100m;
                        var finalPrice = originalTotal - discountAmount;

                        discountInfo = new
                        {
                            code = savedDiscountCode,
                            salePercentage = discount.SalePercentage,
                            originalTotalNumeric = decimal.Round(_currencyService.ConvertFromBaseUsd(originalTotal), 2),
                            discountAmountNumeric = decimal.Round(_currencyService.ConvertFromBaseUsd(discountAmount), 2),
                            finalPriceNumeric = decimal.Round(_currency_service_safe_convert(finalPrice), 2),
                            originalTotalFormatted = _currencyService.Format(originalTotal),
                            discountAmountFormatted = _currencyService.Format(discountAmount),
                            finalPriceFormatted = _currencyService.Format(finalPrice)
                        };
                    }
                    else
                    {
                        Session?.Remove(AppliedDiscountCodeKey);
                    }
                }

                return Json(new
                {
                    success = true,
                    message = "Product added to basket successfully!",
                    totalCount = basket.TotalCount,
                    totalPrice = basket.TotalPrice,
                    totalPriceFormatted = _currencyService.Format(basket.TotalPrice),
                    discount = discountInfo
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

            // local helper to convert safely when _currencyService expects USD base
            decimal _currency_service_safe_convert(decimal baseUsdAmount)
            {
                // _currencyService.ConvertFromBaseUsd multiplies by rate and returns formatted rounding.
                return _currencyService.ConvertFromBaseUsd(baseUsdAmount);
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

            object? discountInfo = null;
            var savedDiscountCode = Session?.GetString(AppliedDiscountCodeKey);
            if (!string.IsNullOrEmpty(savedDiscountCode))
            {
                var discount = await _order_service_getdiscount_safe(savedDiscountCode);
                if (discount != null)
                {
                    var originalTotal = model.TotalPrice;
                    var discountAmount = (originalTotal * discount.SalePercentage) / 100m;
                    var finalPrice = originalTotal - discountAmount;

                    discountInfo = new
                    {
                        code = savedDiscountCode,
                        salePercentage = discount.SalePercentage,
                        originalTotalNumeric = decimal.Round(_currencyService.ConvertFromBaseUsd(originalTotal), 2),
                        discountAmountNumeric = decimal.Round(_currencyService.ConvertFromBaseUsd(discountAmount), 2),
                        finalPriceNumeric = decimal.Round(_currencyService.ConvertFromBaseUsd(finalPrice), 2),
                        originalTotalFormatted = _currencyService.Format(originalTotal),
                        discountAmountFormatted = _currencyService.Format(discountAmount),
                        finalPriceFormatted = _currencyService.Format(finalPrice)
                    };
                }
                else
                {
                    Session?.Remove(AppliedDiscountCodeKey);
                }
            }

            return Json(new
            {
                items = model.Items,
                totalCount = model.TotalCount,
                totalPrice = model.TotalPrice,
                totalPriceFormatted = _currencyService.Format(model.TotalPrice),
                discount = discountInfo
            });


            async Task<dynamic?> _order_service_getdiscount_safe(string code)
            {
                return await _orderService.GetDiscount(code);
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeQuantity(int productVariantId, int change)
        {
            var basketViewModel = await _basketManager.ChangeQuantityAsync(productVariantId, change);

            return Json(new
            {
                success = true,
                basketViewModel,
                totalPriceFormatted = _currencyService.Format(basketViewModel.TotalPrice)
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
                cartHtml,
                totalPriceFormatted = _currencyService.Format(basketViewModel.TotalPrice)
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
            var discountAmount = (originalTotal * discount.SalePercentage) / 100m;
            var finalPrice = originalTotal - discountAmount;

            return Json(new
            {
                success = true,
                salePercentage = discount.SalePercentage,
                originalTotalNumeric = decimal.Round(_currencyService.ConvertFromBaseUsd(originalTotal), 2),
                discountAmountNumeric = decimal.Round(_currencyService.ConvertFromBaseUsd(discountAmount), 2),
                finalPriceNumeric = decimal.Round(_currency_service_safe_convert(finalPrice), 2),
                originalTotalFormatted = _currencyService.Format(originalTotal),
                discountAmountFormatted = _currencyService.Format(discountAmount),
                finalPriceFormatted = _currencyService.Format(finalPrice),
                currency = _currencyService.GetCurrentCurrency().ToString()
            });

            decimal _currency_service_safe_convert(decimal baseUsdAmount) => _currencyService.ConvertFromBaseUsd(baseUsdAmount);
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