using EcommerceCoza.BLL.Services;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.MVC.Controllers
{
    public class OrderController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;
        private readonly BasketManager _basketManager;

        public OrderController(IOrderService orderService, UserManager<AppUser> userManager, IOrderDetailService orderDetailService, BasketManager basketManager)
        {
            _orderService = orderService;
            _userManager = userManager;
            _orderDetailService = orderDetailService;
            _basketManager = basketManager;
        }

        public async Task<IActionResult> Checkout()
        {
            var addressViewModel = new AddressViewModel();
            var model = new OrderCreateViewModel();
            var basketViewModel = await _basketManager.GetBasketAsync();

            model.BasketViewModel = basketViewModel;
            model.OrderDetails = await _orderDetailService.GetOrderDetailCreateViewModels();
            model = await _orderService.GetUserAndAddressViewModel(model);
            model.TotalPrice = basketViewModel.TotalPrice;
            model.EndPrice = basketViewModel.TotalPrice;

            return View(model);
        }

        public async Task<IActionResult> Index()
        {
            var username = User.Identity!.Name ?? "";
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return BadRequest();

            var models = await _orderService.GetOrderViewModelsAsync(user.Id);

            foreach (var model in models)
            {
                model.TotalCount = model.OrderDetails.Sum(x => x.Quantity);
            }

            return View(models);
        }

        public async Task<IActionResult> Details(int id)
        {
            var model = await _orderService.GetAsync(predicate: x => x.Id == id && !x.IsDeleted,
                include: x => x.Include(od => od.OrderDetails).ThenInclude(pv => pv.ProductVariant).ThenInclude(p => p.Product!)
                .Include(od => od.OrderDetails).ThenInclude(pv => pv.ProductVariant).ThenInclude(c => c.Color!)
                .Include(a => a.Address));

            if (model == null)
                return NotFound();

            return View(model);
        }

        public async Task<IActionResult> Confirmation(int id)
        {
            var model = await _orderService.GetAsync(predicate: x => x.Id == id && !x.IsDeleted,
                include: x => x.Include(od => od.OrderDetails)
                    .ThenInclude(pv => pv.ProductVariant)
                    .ThenInclude(p => p.Product!)
                    .Include(od => od.OrderDetails)
                    .ThenInclude(pv => pv.ProductVariant)
                    .ThenInclude(c => c.Color!)
                    .Include(a => a.Address));

            if (model == null)
                return NotFound();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(OrderCreateViewModel model)
        {
            if (model.AddressCreateViewModel == null)
            {
                ModelState.AddModelError("", "Address is required");
                return View(model);
            }

            if (model.AcceptTermsConditions == false)
            {
                ModelState.AddModelError("", "Terms and conditions must be accepted");
                return View(model);
            }

            var basketViewModel = await _basketManager.GetBasketAsync();
            model.OrderDetails = await _orderDetailService.GetOrderDetailCreateViewModels();
            model.BasketViewModel = basketViewModel;

            if (basketViewModel.Items.Count == 0)
            {
                ModelState.AddModelError("", "Your basket is empty");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Discount != null && model.HasAppliedDiscount)
            {
                var discount = await _orderService.GetDiscount(model.Discount);

                if (discount != null)
                {
                    model.DiscountCodeId = discount.Id;
                    model.DiscountAmount = (model.TotalPrice * discount.SalePercentage) / 100;
                    model.EndPrice = model.TotalPrice - model.DiscountAmount;
                }
                else
                {
                    ModelState.AddModelError("", "Invalid discount code");
                    return View(model);
                }
            }
            else
            {
                model.EndPrice = model.TotalPrice;
            }

            // Create the order
            await _orderService.CreateAsync(model);

            // Get the last created order for the current user to retrieve its ID
            var username = User.Identity?.Name ?? "";
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
            {
                return BadRequest("User not found");
            }

            var userOrders = await _orderService.GetOrderViewModelsAsync(user.Id);
            var lastOrder = userOrders.OrderByDescending(o => o.Id).FirstOrDefault();

            if (lastOrder == null)
            {
                return BadRequest("Order creation failed");
            }

            _basketManager.CleanBasket();

            return RedirectToAction("Confirmation", new { id = lastOrder.Id });
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

            var basketViewModel = await _basketManager.GetBasketAsync();
            var discountAmount = (basketViewModel.TotalPrice * discount.SalePercentage) / 100;
            var finalPrice = basketViewModel.TotalPrice - discountAmount;

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