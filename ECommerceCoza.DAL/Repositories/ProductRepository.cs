using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class ProductRepository : EFCoreRepository<Product>, IProductRepository
    {
        public ProductRepository(AppDbContext dbContext) : base(dbContext) { }
    }
}