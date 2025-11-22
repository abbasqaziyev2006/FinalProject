using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class ProductVariantRepository :EFCoreRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(AppDbContext dbContext):base(dbContext) { }
    }
}