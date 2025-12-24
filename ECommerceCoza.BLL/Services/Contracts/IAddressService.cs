using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IAddressService 
        : ICrudService<Address, AddressViewModel, AddressCreateViewModel, AddressUpdateViewModel> 
    {
        Task<Address> CreateAddressAsync(AddressCreateViewModel createViewModel);
    }
}
