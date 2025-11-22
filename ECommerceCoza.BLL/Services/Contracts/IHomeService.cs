using EcommerceCoza.BLL.ViewModels;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetHomeViewModelAsync();
    }
}


