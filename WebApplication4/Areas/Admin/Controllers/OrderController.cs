using EcommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]


    public class OrderController : AdminController
    {
        private readonly IOrderService _orderService;
        private readonly IOrderDetailService _orderDetailService;

        public OrderController(IOrderService orderService, IOrderDetailService orderDetailService)
        {
            _orderService = orderService;
            _orderDetailService = orderDetailService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync(include: x =>
            x.Include(o => o.OrderDetails).ThenInclude(p => p.ProductVariant)
            .Include(a => a.Address)
            .Include(u => u.AppUser!));

            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _orderService.GetDetailsOfOrderAsync(id);

            return View(order);
        }
    }
}
