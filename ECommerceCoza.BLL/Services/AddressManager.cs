using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;


namespace EcommerceCoza.BLL.Services
{
    public class AddressManager : CrudManager<Address, AddressViewModel, AddressCreateViewModel, AddressUpdateViewModel>,
        IAddressService
    {
        public AddressManager(IRepository<Address> repository, IMapper mapper) : base(repository, mapper)
        {

        }


        public async Task SetDefaultAddressOfUser(int addressId, string userId) 
        {
            var addresses = await GetAllAsync(
                predicate: x => x.AppUserId == userId && !x.IsDeleted && x.Id!=addressId);

            if (addresses.Any())
            {
                foreach (var item in addresses)
                {
                    item.IsDefault = false;
                }
            }

            var address = await GetAsync(predicate: x => x.Id == addressId);
            
            if (address != null)
            {
                address.IsDefault = true;
            }

        }

        public async Task<Address> CreateAddressAsync(AddressCreateViewModel createViewModel)
        {
            var address = Mapper.Map<Address>(createViewModel);

            //if (userId != null)
            //{
            //    address.AppUserId = userId;

            //   await  SetDefaultAddressOfUser(address.Id, userId);
            //}

            await Repository.CreateAsync(address);

            return address;
        }

    }
}
