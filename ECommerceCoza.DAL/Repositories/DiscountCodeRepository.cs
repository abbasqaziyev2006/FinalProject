using EcommerceCoza.DAL.DataContext.Repositories;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class DiscountCodeRepository : EFCoreRepository<DiscountCode>, IDiscountCodeRepository
    {
        public DiscountCodeRepository(AppDbContext dbContext) : base(dbContext) { }
    }
}