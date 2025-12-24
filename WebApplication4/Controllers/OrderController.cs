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
            // 1. Səbət və Detalları gətir
            var model = new OrderCreateViewModel
            {
                BasketViewModel = await _basketManager.GetBasketAsync(),
                OrderDetails = await _orderDetailService.GetOrderDetailCreateViewModels()
            };

            // 2. İstifadəçi və Adres məlumatlarını doldur
            model = await _orderService.GetUserAndAddressViewModel(model);

            // 3. Qiymət hesablaması (TAX YOXDUR)
            model.TotalPrice = model.BasketViewModel.TotalPrice;
            model.EndPrice = model.TotalPrice;

            // 4. Endirim varsa tətbiq et
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

            // --- N/A PROBLEMİNİN HƏLLİ (Addım 1) ---
            // Formdan gəlməsə belə, User məlumatlarını məcburi doldururuq
            var user = await _userManager.GetUserAsync(User);
            model.AppUserId = user.Id;
            model.Email = user.Email;
            model.FirstName = user.FirstName; // ViewModel-də varsa
            model.LastName = user.LastName;   // ViewModel-də varsa
            model.PhoneNumber = user.PhoneNumber; // ViewModel-də varsa

            model.BasketViewModel = basket;
            model.TotalPrice = basket.TotalPrice;

            // Endirim hesablama
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

            // STRIPE ÖDƏNİŞİ
            if (model.PaymentMethod == ECommerceCoza.DAL.DataContext.Entities.PaymentMethod.Stripe)
            {
                var orderToken = Guid.NewGuid().ToString();

                // Valyuta Çevrimi (AZN -> USD)
                var currentCurrency = _currencyService.GetCurrentCurrency();
                decimal stripeAmount = model.EndPrice;

                // Stripe AZN dəstəkləmədiyi üçün USD-yə çeviririk
                if (currentCurrency == Currency.AZN)
                {
                    stripeAmount = _currencyService.ConvertBetweenCurrencies(model.EndPrice, Currency.AZN, Currency.USD);
                }

                long totalAmountInCents = (long)Math.Round(stripeAmount * 100);

                // Modeli sessiyaya yazırıq (Artıq User məlumatları içindədir)
                var serializerSettings = new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };
                Session.SetString(orderToken, JsonConvert.SerializeObject(model, serializerSettings));

                var options = new SessionCreateOptions
                {
                    PaymentMethodTypes = new List<string> { "card" },
                    Mode = "payment",
                    LineItems = new List<SessionLineItemOptions> {
                        new SessionLineItemOptions {
                            PriceData = new SessionLineItemPriceDataOptions {
                                Currency = "usd", // Həmişə USD gedir (konvertasiya olunub)
                                UnitAmount = totalAmountInCents,
                                ProductData = new SessionLineItemPriceDataProductDataOptions { Name = $"Order Payment ({currentCurrency})" }
                            },
                            Quantity = 1
                        }
                    },
                    SuccessUrl = $"{Request.Scheme}://{Request.Host}/Order/StripeSuccess?session_id={{CHECKOUT_SESSION_ID}}",
                    CancelUrl = $"{Request.Scheme}://{Request.Host}/Order/Checkout",
                    // User ID-ni Metadata-da daşıyırıq (Ehtiyat üçün)
                    Metadata = new Dictionary<string, string> { { "OrderToken", orderToken }, { "UserId", user.Id } }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);
                return Redirect(session.Url);
            }

            // NAĞD ÖDƏNİŞ
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

                // --- N/A PROBLEMİNİN HƏLLİ (Addım 2 - Final) ---
                // Sessiyada nəsə itibsə, Metadata-dan bərpa edirik
                var user = await _userManager.FindByIdAsync(session.Metadata["UserId"]);

                model.AppUserId = user.Id;
                model.Email = user.Email;
                model.FirstName ??= user.FirstName; // Əgər boşdursa user-dən götür
                model.LastName ??= user.LastName;
                model.PhoneNumber ??= user.PhoneNumber;

                model.PaymentMethod = ECommerceCoza.DAL.DataContext.Entities.PaymentMethod.Stripe;
                model.OrderStatus = OrderStatus.InProgress;

                // Veritabanına yaz
                await _orderService.CreateAsync(model);

                // Təmizlik
                _basketManager.CleanBasket();
                Session?.Remove(orderToken);
                Session?.Remove(AppliedDiscountCodeKey);

                TempData["Success"] = "Ödəniş uğurlu oldu!";
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

            // DİQQƏT: GetOrderViewModelsAsync metodu daxilində mütləq .Include(x => x.Address) olmalıdır.
            // Əgər servisdə yoxdursa, aşağıdakı dövr daxilində address-in null olmadığını yoxlayın.
            var orders = await _orderService.GetOrderViewModelsAsync(user.Id);

            foreach (var order in orders)
            {
                // Detalları hesablayırıq
                order.TotalCount = order.OrderDetails?.Sum(x => x.Quantity) ?? 0;

                // Əgər Address null gəlirsə, bazadan təkrar Include ilə çəkməyi servisdə təmin edin.
            }

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetAsync(
                predicate: x => x.Id == id && !x.IsDeleted,
                include: x => x.Include(o => o.OrderDetails)
                                .ThenInclude(od => od.ProductVariant)
                                    .ThenInclude(pv => pv.Product!) // Məhsul adı üçün
                                .Include(o => o.OrderDetails)
                                .ThenInclude(od => od.ProductVariant)
                                    .ThenInclude(pv => pv.Color)   // <--- RƏNG ÜÇÜN BU VACİBDİR
                                .Include(o => o.Address));

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyDiscount(string discountCode)
        {
            var discount = await _orderService.GetDiscount(discountCode);
            if (discount == null) return Json(new { success = false });

            Session?.SetString(AppliedDiscountCodeKey, discountCode);
            var basket = await _basketManager.GetBasketAsync();

            // Tax olmadan sadə hesablama
            var discountAmount = (basket.TotalPrice * discount.SalePercentage) / 100;

            return Json(new
            {
                success = true,
                discountAmount = Math.Round(discountAmount, 2),
                finalPrice = Math.Round(basket.TotalPrice - discountAmount, 2)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id) // Adı sadəcə Cancel etdik
        {
            var order = await _orderService.GetAsync(predicate: x => x.Id == id);
            if (order == null) return NotFound();

            if (order.OrderStatus == OrderStatus.Completed || order.OrderStatus == OrderStatus.Cancelled)
            {
                TempData["Error"] = "Bu sifarişi ləğv etmək olmaz.";
                return RedirectToAction("Details", new { id });
            }

            var updateModel = new OrderUpdateViewModel
            {
                Id = id,
                OrderStatus = OrderStatus.Cancelled,
                CanceledDate = DateTime.UtcNow
            };

            await _orderService.UpdateAsync(id, updateModel);
            TempData["Success"] = "Sifariş uğurla ləğv edildi.";
            return RedirectToAction("Index"); // Ləğvdən sonra siyahıya qayıt
        }
    }
}