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

        private ISession? Session => _accessor.HttpContext?.Session;
        private const string AppliedDiscountCodeKey = "AppliedDiscountCode";

        [HttpGet]
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

            var savedDiscountCode = Session?.GetString(AppliedDiscountCodeKey);
            if (!string.IsNullOrEmpty(savedDiscountCode))
            {
                var discount = await _orderService.GetDiscount(savedDiscountCode);
                if (discount != null)
                {
                    model.HasAppliedDiscount = true;
                    model.Discount = savedDiscountCode;
                    model.DiscountCodeId = discount.Id;
                    model.DiscountAmount = (model.TotalPrice * discount.SalePercentage) / 100;
                    model.EndPrice = model.TotalPrice - model.DiscountAmount;
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(OrderCreateViewModel model)
        {
            var basket = await _basketManager.GetBasketAsync();
            if (basket.Items.Count == 0) return RedirectToAction("Index", "Shop");

        
            var user = await _userManager.GetUserAsync(User);
            model.AppUserId = user.Id;
            model.Email = user.Email;
            model.FirstName = user.FirstName;
            model.LastName = user.LastName;   
            model.PhoneNumber = user.PhoneNumber; 

            model.BasketViewModel = basket;
            model.TotalPrice = basket.TotalPrice;

          
            if (model.HasAppliedDiscount && !string.IsNullOrEmpty(model.Discount))
            {
                var d = await _orderService.GetDiscount(model.Discount);
                if (d != null)
                {
                    model.DiscountCodeId = d.Id;
                    model.DiscountAmount = (model.TotalPrice * d.SalePercentage) / 100;
                    model.EndPrice = model.TotalPrice - model.DiscountAmount;
                }
            }
            else { model.EndPrice = model.TotalPrice; }


            if (model.PaymentMethod == ECommerceCoza.DAL.DataContext.Entities.PaymentMethod.Stripe)
            {
                var orderToken = Guid.NewGuid().ToString();

             
                var currentCurrency = _currencyService.GetCurrentCurrency();
                decimal stripeAmount = model.EndPrice;

              
                if (currentCurrency == Currency.AZN)
                {
                    stripeAmount = _currencyService.ConvertBetweenCurrencies(model.EndPrice, Currency.AZN, Currency.USD);
                }

                long totalAmountInCents = (long)Math.Round(stripeAmount * 100);

               
                var serializerSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                Session.SetString(orderToken, JsonConvert.SerializeObject(model, serializerSettings));

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    Mode = "payment",
                    LineItems = new List<SessionLineItemOptions> {
                        new SessionLineItemOptions {
                            PriceData = new SessionLineItemPriceDataOptions {
                                Currency = "usd",
                                UnitAmount = totalAmountInCents,
                                ProductData = new SessionLineItemPriceDataProductDataOptions { Name = $"Order Payment ({currentCurrency})" }
                            },
                            Quantity = 1
                        }
                    },
                    SuccessUrl = $"{Request.Scheme}://{Request.Host}/Order/StripeSuccess?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/Order/Checkout",
                 
                    Metadata = new Dictionary<string, string> { { "OrderToken", orderToken }, { "UserId", user.Id } }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);
                return Redirect(session.Url);
            }

            
            await _orderService.CreateAsync(model);
            _basketManager.CleanBasket();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> StripeSuccess(string session_id)
        {
            if (string.IsNullOrEmpty(session_id) || session_id == "{CHECKOUT_SESSION_ID}") return RedirectToAction("Checkout");

            try
            {
                var service = new SessionService();
                var session = await service.GetAsync(session_id);
                if (session.PaymentStatus != "paid") return RedirectToAction("Checkout");

                var orderToken = session.Metadata["OrderToken"];
                var json = Session?.GetString(orderToken);
                if (string.IsNullOrWhiteSpace(json)) return RedirectToAction("Index", "Shop");

                var model = JsonConvert.DeserializeObject<OrderCreateViewModel>(json);

                var user = await _userManager.FindByIdAsync(session.Metadata["UserId"]);

                model.AppUserId = user.Id;
                model.Email = user.Email;
                model.FirstName ??= user.FirstName; 
                model.LastName ??= user.LastName;
                model.PhoneNumber ??= user.PhoneNumber;

                model.PaymentMethod = ECommerceCoza.DAL.DataContext.Entities.PaymentMethod.Stripe;
                model.OrderStatus = OrderStatus.InProgress;

          
                await _orderService.CreateAsync(model);

                _basketManager.CleanBasket();
                Session?.Remove(orderToken);
                Session?.Remove(AppliedDiscountCodeKey);

                TempData["Success"] = "Payment was successful!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Stripe Success Error");
                return RedirectToAction("Index", "Shop");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

   
            var orders = await _orderService.GetOrderViewModelsAsync(user.Id);

            foreach (var order in orders)
            {
       
                order.TotalCount = order.OrderDetails?.Sum(x => x.Quantity) ?? 0;

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
                                    .ThenInclude(pv => pv.Color)   
                                .Include(o => o.Address));

            if (order == null) return NotFound();
            return View(order);
        }


      [HttpPost]
      [ValidateAntiForgeryToken]
public async Task<IActionResult> ApplyDiscount(string discountCode)
        {
            if (string.IsNullOrWhiteSpace(discountCode))
                return Json(new { success = false, message = "Please enter a discount code" });

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id) 
        {
            var order = await _orderService.GetAsync(predicate: x => x.Id == id);
            if (order == null) return NotFound();

            if (order.OrderStatus == OrderStatus.Completed || order.OrderStatus == OrderStatus.Cancelled)
            {
                TempData["Error"] = "This order cannot be cancelled.";
                return RedirectToAction("Details", new { id });
            }

            var updateModel = new OrderUpdateViewModel
            {
                Id = id,
                OrderStatus = OrderStatus.Cancelled,
                CanceledDate = DateTime.UtcNow
            };

            await _orderService.UpdateAsync(id, updateModel);
            TempData["Success"] = "Order cancelled successfully.";
            return RedirectToAction("Index"); 
        }
    }
}