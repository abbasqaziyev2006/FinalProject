using EcommerceCoza.DAL.DataContext.Repositories;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class CategoryRepository : EFCoreRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(AppDbContext dbContext):base(dbContext)
        {
            
        }
    }
}