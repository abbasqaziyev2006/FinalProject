using EcommerceCoza.BLL.ViewModels;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IFooterService
    {
        Task<FooterViewModel> GetFooterViewModelAsync();
    }
}
