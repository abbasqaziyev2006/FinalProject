using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IOrderService _orderService;

        public DashboardController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderService.GetAllAsync(
                include: x => x
                    .Include(o => o.OrderDetails).ThenInclude(d => d.ProductVariant)
                    .Include(o => o.Address)
                    .Include(o => o.AppUser)
            );

         
            foreach (var o in orders)
            {
              
                o.TotalCount = o.OrderDetails?.Sum(d => d.Quantity) ?? 0;
            }

            var model = new DashboardViewModel();

            model.TotalOrders = orders.Count();
            model.TotalAmount = orders.Sum(o => o.TotalPrice);

            bool IsPending(OrderViewModel o) =>
                o.OrderStatus == OrderStatus.OnHold || o.OrderStatus == OrderStatus.InProgress;

            model.PendingOrdersCount = orders.Count(IsPending);
            model.PendingOrdersAmount = orders.Where(IsPending).Sum(o => o.TotalPrice);

            model.DeliveredOrdersCount = orders.Count(o => o.OrderStatus == OrderStatus.Delivered);
            model.DeliveredOrdersAmount = orders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalPrice);

            model.CancelledOrdersCount = orders.Count(o => o.OrderStatus == OrderStatus.Cancelled);
            model.CancelledOrdersAmount = orders.Where(o => o.OrderStatus == OrderStatus.Cancelled).Sum(o => o.TotalPrice);

            
            for (int month = 1; month <= 12; month++)
            {
                var monthOrders = orders.Where(o => o.CreatedAt?.Month == month).ToList();
                model.TotalSeries.Add(monthOrders.Sum(o => o.TotalPrice));
                model.PendingSeries.Add(monthOrders.Where(IsPending).Sum(o => o.TotalPrice));
                model.DeliveredSeries.Add(monthOrders.Where(o => o.OrderStatus == OrderStatus.Delivered).Sum(o => o.TotalPrice));
                model.CancelledSeries.Add(monthOrders.Where(o => o.OrderStatus == OrderStatus.Cancelled).Sum(o => o.TotalPrice));
            }

            model.RecentOrders = orders.OrderByDescending(o => o.CreatedAt).Take(5).ToList();

            return View(model);
        }
    }
}