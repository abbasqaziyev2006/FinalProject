using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.DAL.DataContext.Repositories
{
    public class AddressRepository :EFCoreRepository<Address>, IAddressRepository
    {
        public AddressRepository(AppDbContext dbContext):base(dbContext) { }
    }
}