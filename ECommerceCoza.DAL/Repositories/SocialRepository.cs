using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class SocialRepository : EFCoreRepository<Social>, ISocialRepository
    {
        public SocialRepository(AppDbContext dbContext) : base(dbContext)
        {

        }
    }
}