using ECommerceCoza.DAL.DataContext.Entities;
using System.Collections.Generic;

namespace EcommerceCoza.BLL.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalAmount { get; set; }

        public int PendingOrdersCount { get; set; }
        public decimal PendingOrdersAmount { get; set; }

        public int DeliveredOrdersCount { get; set; }
        public decimal DeliveredOrdersAmount { get; set; }

        public int CancelledOrdersCount { get; set; }
        public decimal CancelledOrdersAmount { get; set; }

        // Monthly series (Jan .. Dec)
        public List<decimal> TotalSeries { get; set; } = new();
        public List<decimal> PendingSeries { get; set; } = new();
        public List<decimal> DeliveredSeries { get; set; } = new();
        public List<decimal> CancelledSeries { get; set; } = new();

        public List<OrderViewModel> RecentOrders { get; set; } = new();
    }
}