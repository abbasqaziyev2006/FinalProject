using EcommerceCoza.BLL.Services;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Stripe;
using Stripe.Checkout;
using WebApplication4.Models;
using WebApplication4.Services;

namespace EcommerceCoza.MVC.Controllers
{
    [Authorize] 
    public class OrderController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly BasketManager _basketManager;
        private readonly ICurrencyService _currencyService;
        private readonly StripeSettings _stripeSettings;
        private readonly IHttpContextAccessor _accessor;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService orderService,
            UserManager<AppUser> userManager,
            IOrderDetailService orderDetailService,
            BasketManager basketManager,
            ICurrencyService currencyService,
            IOptions<StripeSettings> stripeSettings,
            IHttpContextAccessor accessor,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _userManager = userManager;
            _orderDetailService = orderDetailService;
            _basketManager = basketManager;
            _currencyService = currencyService;
            _stripeSettings = stripeSettings.Value;
            _accessor = accessor;
            _logger = logger;

            StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
        }

        private ISession Session => _accessor.HttpContext!.Session;

        public async Task<IActionResult> Checkout()
        {
            var model = new OrderCreateViewModel
            {
                BasketViewModel = await _basketManager.GetBasketAsync(),
                OrderDetails = await _orderDetailService.GetOrderDetailCreateViewModels()
            };

            model = await _orderService.GetUserAndAddressViewModel(model);
            model.TotalPrice = model.BasketViewModel.TotalPrice;
            model.EndPrice = model.TotalPrice;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderCreateViewModel model)
        {
            if (model.AddressCreateViewModel == null)
            {
                ModelState.AddModelError("", "Address is required");
            }

            if (!model.AcceptTermsConditions)
            {
                ModelState.AddModelError("", "Terms and conditions must be accepted");
            }

            var basket = await _basketManager.GetBasketAsync();
            model.BasketViewModel = basket;
            model.OrderDetails = await _orderDetailService.GetOrderDetailCreateViewModels();

            if (basket.Items.Count == 0)
            {
                ModelState.AddModelError("", "Your basket is empty");
            }

            if (!ModelState.IsValid)
            {
                model = await _orderService.GetUserAndAddressViewModel(model);
                return View(model);
            }

            // İndirim kontrolü
            if (model.HasAppliedDiscount && model.Discount != null)
            {
                var d = await _orderService.GetDiscount(model.Discount);
                if (d == null)
                {
                    ModelState.AddModelError("", "Invalid discount code");
                    model = await _orderService.GetUserAndAddressViewModel(model);
                    return View(model);
                }

                model.DiscountCodeId = d.Id;
                model.DiscountAmount = (model.TotalPrice * d.SalePercentage) / 100;
                model.EndPrice = model.TotalPrice - model.DiscountAmount;
            }
            else
            {
                model.EndPrice = model.TotalPrice;
            }

            // ✅ STRIPE ÖDEME
            if (model.PaymentMethod == ECommerceCoza.DAL.DataContext.Entities.PaymentMethod.Stripe)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    _logger.LogError("User not found during checkout");
                    return BadRequest("User not found");
                }

                var orderToken = Guid.NewGuid().ToString();

                // ✅ Güvenli serileştirme
                var serializerSettings = new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore
                };
                Session.SetString(orderToken, JsonConvert.SerializeObject(model, serializerSettings));

                // ✅ Currency conversion
                var currentCurrency = _currencyService.GetCurrentCurrency();
                var amountInUsd = currentCurrency == Currency.USD
                    ? model.EndPrice
                    : _currencyService.ConvertBetweenCurrencies(model.EndPrice, currentCurrency, Currency.USD);

                var stripeCurrency = "usd"; // Stripe'a her zaman USD gönderiyoruz

                try
                {
                    var options = new SessionCreateOptions
                    {
                        PaymentMethodTypes = new List<string> { "card" },
                        Mode = "payment",
                        LineItems = new List<SessionLineItemOptions>
                        {
                            new SessionLineItemOptions
                            {
                                PriceData = new SessionLineItemPriceDataOptions
                                {
                                    Currency = stripeCurrency,
                                    UnitAmount = (long)(amountInUsd * 100),
                                    ProductData = new SessionLineItemPriceDataProductDataOptions
                                    {
                                        Name = "Order Payment",
                                        Description = $"Payment for {basket.Items.Count} items"
                                    }
                                },
                                Quantity = 1
                            }
                        },
                        SuccessUrl = Url.Action("StripeSuccess", "Order", null, Request.Scheme)
                                      + "?session_id={CHECKOUT_SESSION_ID}",
                        CancelUrl = Url.Action("Checkout", "Order", null, Request.Scheme),
                        Metadata = new Dictionary<string, string>
                        {
                            { "OrderToken", orderToken },
                            { "UserId", user.Id }
                        }
                    };

                    var service = new SessionService();
                    var session = await service.CreateAsync(options);

                    return Redirect(session.Url);
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Stripe payment error");
                    ModelState.AddModelError("", $"Payment error: {ex.Message}");
                    model = await _orderService.GetUserAndAddressViewModel(model);
                    return View(model);
                }
            }

            // ✅ DİĞER ÖDEME YÖNTEMLERİ
            await _orderService.CreateAsync(model);

            var username = User.Identity?.Name ?? "";
            var currentUser = await _userManager.FindByNameAsync(username);

            if (currentUser == null)
            {
                _logger.LogError("User not found after order creation");
                return BadRequest("User not found");
            }

            var userOrders = await _orderService.GetOrderViewModelsAsync(currentUser.Id);
            var lastOrder = userOrders
                .Where(o => o.CreatedAt >= DateTime.UtcNow.AddMinutes(-5))
                .OrderByDescending(o => o.Id)
                .FirstOrDefault();

            if (lastOrder == null)
            {
                _logger.LogError("Order creation failed - no recent order found");
                return BadRequest("Order creation failed");
            }

            _basketManager.CleanBasket();

            return RedirectToAction("Confirmation", new { id = lastOrder.Id });
        }

        [HttpGet]
        public async Task<IActionResult> StripeSuccess(string session_id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(session_id))
                {
                    TempData["Error"] = "Invalid payment session";
                    return RedirectToAction("Checkout");
                }

                var service = new SessionService();
                var session = await service.GetAsync(session_id);

                if (session.PaymentStatus != "paid")
                {
                    TempData["Error"] = "Payment was not completed successfully";
                    return RedirectToAction("Checkout");
                }

                var orderToken = session.Metadata["OrderToken"];
                var userId = session.Metadata["UserId"];

                var json = Session.GetString(orderToken);
                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger.LogError("Order data not found in session for token: {OrderToken}", orderToken);
                    TempData["Error"] = "Session expired. Please try again.";
                    return RedirectToAction("Checkout");
                }

                var model = JsonConvert.DeserializeObject<OrderCreateViewModel>(json);
                if (model == null)
                {
                    _logger.LogError("Failed to deserialize order data");
                    TempData["Error"] = "Invalid order data";
                    return RedirectToAction("Checkout");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    _logger.LogError("User not found: {UserId}", userId);
                    TempData["Error"] = "User not found";
                    return RedirectToAction("Checkout");
                }

                // ✅ Duplicate order önleme
                var existingOrder = await _orderService.GetAsync(
                    predicate: x => x.AppUserId == userId &&
                                    x.EndPrice == model.EndPrice &&
                                    x.CreatedAt >= DateTime.UtcNow.AddMinutes(-5));

                if (existingOrder != null)
                {
                    _logger.LogInformation("Order already exists, redirecting to confirmation");
                    _basketManager.CleanBasket();
                    Session.Remove(orderToken);
                    return RedirectToAction("Confirmation", new { id = existingOrder.Id });
                }

                await _orderService.CreateAsync(model);

                var userOrders = await _orderService.GetOrderViewModelsAsync(userId);
                var lastOrder = userOrders
                    .Where(o => o.CreatedAt >= DateTime.UtcNow.AddMinutes(-5))
                    .OrderByDescending(o => o.Id)
                    .FirstOrDefault();

                if (lastOrder == null)
                {
                    _logger.LogError("Order creation failed");
                    TempData["Error"] = "Order creation failed";
                    return RedirectToAction("Checkout");
                }

                _basketManager.CleanBasket();
                Session.Remove(orderToken);

                return RedirectToAction("Confirmation", new { id = lastOrder.Id });
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe verification error");
                TempData["Error"] = "Payment verification failed. Please contact support.";
                return RedirectToAction("Checkout");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during payment success callback");
                TempData["Error"] = "An unexpected error occurred. Please contact support.";
                return RedirectToAction("Checkout");
            }
        }

        public async Task<IActionResult> Index()
        {
            var username = User.Identity?.Name ?? "";
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return BadRequest("User not found");

            var orders = await _orderService.GetOrderViewModelsAsync(user.Id);

            foreach (var order in orders)
            {
                order.TotalCount = order.OrderDetails.Sum(x => x.Quantity);
            }

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetAsync(
                predicate: x => x.Id == id && !x.IsDeleted,
                include: x => x.Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                    .ThenInclude(pv => pv.Product!)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                    .ThenInclude(pv => pv.Color!)
                    .Include(o => o.Address));

            if (order == null)
                return NotFound();

            // ✅ Kullanıcı kendi siparişine erişiyor mu kontrol et
            var user = await _userManager.GetUserAsync(User);
            if (order.AppUserId != user?.Id && !User.IsInRole("Admin"))
                return Forbid();

            return View(order);
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var order = await _orderService.GetAsync(
                predicate: x => x.Id == id && !x.IsDeleted,
                include: x => x.Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                    .ThenInclude(pv => pv.Product!)
                    .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.ProductVariant)
                    .ThenInclude(pv => pv.Color!)
                    .Include(o => o.Address));

            if (order == null)
                return NotFound();

            // ✅ Kullanıcı kendi siparişine erişiyor mu kontrol et
            var user = await _userManager.GetUserAsync(User);
            if (order.AppUserId != user?.Id && !User.IsInRole("Admin"))
                return Forbid();

            return View(order);
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
                return Json(new
                {
                    success = false,
                    message = "Invalid or expired discount code"
                });
            }

            var basket = await _basketManager.GetBasketAsync();
            var discountAmount = (basket.TotalPrice * discount.SalePercentage) / 100;
            var finalPrice = basket.TotalPrice - discountAmount;

            return Json(new
            {
                success = true,
                salePercentage = discount.SalePercentage,
                discountAmount = Math.Round(discountAmount, 2),
                finalPrice = Math.Round(finalPrice, 2)
            });
        }
    }
}