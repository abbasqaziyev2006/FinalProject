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
using Stripe.Climate;
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
        }

        private ISession? Session => _accessor.HttpContext?.Session;
        private const string AppliedDiscountCodeKey = "AppliedDiscountCode";

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

            // Read saved discount code from session and apply if present
            var savedDiscountCode = Session?.GetString(AppliedDiscountCodeKey);
            if (!string.IsNullOrEmpty(savedDiscountCode))
            {
                ViewData["SavedDiscountCode"] = savedDiscountCode;

                var discount = await _orderService.GetDiscount(savedDiscountCode);
                if (discount != null)
                {
                    model.HasAppliedDiscount = true;
                    model.Discount = savedDiscountCode;
                    model.DiscountCodeId = discount.Id;
                    model.DiscountAmount = (model.TotalPrice * discount.SalePercentage) / 100;
                    model.EndPrice = model.TotalPrice - model.DiscountAmount;
                    ViewData["DiscountPercentage"] = discount.SalePercentage;
                }
                else
                {
                    Session?.Remove(AppliedDiscountCodeKey);
                }
            }

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderCreateViewModel model)
        {
            // 1. Səbəti və detalları yenidən yükləyirik (təhlükəsizlik üçün)
            var basket = await _basketManager.GetBasketAsync();
            model.BasketViewModel = basket;
            model.OrderDetails = await _orderDetailService.GetOrderDetailCreateViewModels();

            if (basket.Items.Count == 0)
            {
                ModelState.AddModelError("", "Your basket is empty");
                return View(model);
            }

            // 2. Məbləği Hesablayırıq (EndPrice mütləq dolmalıdır)
            model.TotalPrice = basket.TotalPrice;

            if (model.HasAppliedDiscount && model.Discount != null)
            {
                var d = await _orderService.GetDiscount(model.Discount);
                if (d != null)
                {
                    model.DiscountCodeId = d.Id;
                    model.DiscountAmount = (model.TotalPrice * d.SalePercentage) / 100;
                    model.EndPrice = model.TotalPrice - model.DiscountAmount;
                }
            }
            else
            {
                model.EndPrice = model.TotalPrice;
            }

            // MƏBLƏĞİN SIFIR OLMADIĞINI YOXLAYIN
            if (model.EndPrice <= 0)
            {
                ModelState.AddModelError("", "Invalid order amount.");
                return View(model);
            }

            // 3. Stripe Flow
            if (model.PaymentMethod == ECommerceCoza.DAL.DataContext.Entities.PaymentMethod.Stripe)
            {
                var user = await _userManager.GetUserAsync(User);
                var orderToken = Guid.NewGuid().ToString();

                // Modeli session-a yazırıq (Stripe-dan qayıdanda istifadə üçün)
                var serializerSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                Session.SetString(orderToken, JsonConvert.SerializeObject(model, serializerSettings));

                // Stripe üçün qəpik hesabı (Məsələn: 10.50 AZN -> 1050)
                // Birbaşa AZN göndərmək daha etibarlıdır
                long totalAmountInCents = (long)Math.Round(model.EndPrice * 100);

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
                        Currency = "azn", // Birbaşa AZN istifadə edirik
                        UnitAmount = totalAmountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Order Payment",
                            Description = $"Payment for {basket.Items.Count} items"
                        }
                    },
                    Quantity = 1
                }
            },
                    SuccessUrl = Url.Action("StripeSuccess", "Order", new { session_id = "{CHECKOUT_SESSION_ID}" }, Request.Scheme),
                    CancelUrl = Url.Action("Checkout", "Order", null, Request.Scheme),
                    Metadata = new Dictionary<string, string>
            {
                { "OrderToken", orderToken },
                { "UserId", user.Id }
            }
                };

                var service = new SessionService();
                try
                {
                    var session = await service.CreateAsync(options);
                    return Redirect(session.Url);
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Stripe API Error");
                    ModelState.AddModelError("", "Payment error: " + ex.Message);
                    return View(model);
                }
            }

            // Əgər nağd ödənişdirsə:
            await _orderService.CreateAsync(model);
            _basketManager.CleanBasket();
            return RedirectToAction("Index");
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

                var existingOrder = await _orderService.GetAsync(
                    predicate: x => x.AppUserId == userId &&
                                    x.EndPrice == model.EndPrice &&
                                    x.CreatedAt >= DateTime.UtcNow.AddMinutes(-5));

                if (existingOrder != null)
                {
                    _logger.LogInformation("Order already exists, redirecting to confirmation");
                    _basketManager.CleanBasket();
                    Session?.Remove(orderToken);
                    Session?.Remove(AppliedDiscountCodeKey);
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
                Session?.Remove(orderToken);
                Session?.Remove(AppliedDiscountCodeKey);

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
                Session?.Remove(AppliedDiscountCodeKey);
                return Json(new
                {
                    success = false,
                    message = "Invalid or expired discount code"
                });
            }

            Session?.SetString(AppliedDiscountCodeKey, discountCode);

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

        // Debug / UX helper — graceful GET redirect if someone visits /Order/Cancel directly
        [HttpGet]
        public IActionResult Cancel(int id)
        {
            // If user opens /Order/Cancel in browser, send them back to details with a message
            TempData["Error"] = "Please cancel orders from the Order Details page (use the Cancel button).";
            return RedirectToAction("Details", new { id });
        }

        // Ensure POST route explicitly matches /Order/Cancel and keep antiforgery validation
        [HttpPost]
        [Route("Order/Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelPost(int id)
        {
            var order = await _orderService.GetAsync(predicate: x => x.Id == id && !x.IsDeleted);
            if (order == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (order.AppUserId != user?.Id && !User.IsInRole("Admin"))
                return Forbid();

            if (order.OrderStatus == OrderStatus.Cancelled || order.OrderStatus == OrderStatus.Completed)
            {
                TempData["Error"] = "Order cannot be cancelled.";
                return RedirectToAction("Details", new { id });
            }

            var updateModel = new OrderUpdateViewModel
            {
                Id = id,
                OrderStatus = OrderStatus.Cancelled,
                CanceledDate = DateTime.UtcNow
            };

            var success = await _orderService.UpdateAsync(id, updateModel);
            if (!success)
            {
                TempData["Error"] = "Failed to cancel order. Please try again later.";
                return RedirectToAction("Details", new { id });
            }

            TempData["Success"] = "Order cancelled successfully.";
            return RedirectToAction("Details", new { id });
        }
    }
}