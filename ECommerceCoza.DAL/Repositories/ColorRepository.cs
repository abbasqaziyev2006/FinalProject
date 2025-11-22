using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class ColorRepository : EFCoreRepository<Color>, IColorRepository
    {
        public ColorRepository(AppDbContext dbContext) : base(dbContext)
        {

        }
    }
}