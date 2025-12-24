using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IProductVariantService : ICrudService<ProductVariant, ProductVariantViewModel, ProductVariantCreateViewModel, ProductVariantUpdateViewModel>
    {
        Task<ProductVariantCreateViewModel> GetCreateViewModelAsync();
        Task<ProductVariantUpdateViewModel> GetProductVariantUpdateViewModelAsync(int id);
    }

}
