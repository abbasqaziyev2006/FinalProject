using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class BioRepository : EFCoreRepository<Bio>, IBioRepository
    {
        public BioRepository(AppDbContext dbContext) : base(dbContext)
        {

        }
    }
}