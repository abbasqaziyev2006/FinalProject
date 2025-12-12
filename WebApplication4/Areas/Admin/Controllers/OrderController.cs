using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.MVC.Areas.Admin.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

    public IActionResult Tracking()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Tracking(int orderId, string email)
    {
        if (orderId <= 0 || string.IsNullOrWhiteSpace(email))
        {
            TempData["Error"] = "Please provide both Order ID and Email";
            return View();
        }

        var order = await _orderService.GetAsync(
            predicate: x => x.Id == orderId && x.Email == email && !x.IsDeleted,
            include: x => x.Include(o => o.OrderDetails)
                .ThenInclude(od => od.ProductVariant)
                .ThenInclude(pv => pv.Product!)
                .Include(o => o.Address)
                .Include(o => o.AppUser!));

        if (order == null)
        {
            TempData["Error"] = "Order not found. Please check your Order ID and Email.";
            return View();
        }

        return View(order);
    }
}