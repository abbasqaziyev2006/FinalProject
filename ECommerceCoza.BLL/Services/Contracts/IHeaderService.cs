using EcommerceCoza.BLL.ViewModels;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IHeaderService
    {
        Task<HeaderViewModel> GetHeaderViewModelAsync();
    }
}
