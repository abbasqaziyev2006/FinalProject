using EcommerceCoza.BLL.ViewModels;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IShopService
    {
        Task<ShopViewModel> GetShopViewModelAsync();
    }
}
