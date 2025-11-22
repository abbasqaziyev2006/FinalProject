using EcommerceCoza.DAL.DataContext.Repositories;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class OrderDetailRepository :EFCoreRepository<OrderDetail>, IOrderDetailRepository
    {
        public OrderDetailRepository (AppDbContext dbContext) : base(dbContext) { }
    }
}