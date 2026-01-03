using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using static EcommerceCoza.BLL.ViewModels.ProductViewModel;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IProductService : ICrudService<Product, ProductViewModel, ProductCreateViewModel, ProductUpdateViewModel>
    {
        Task<ProductCreateViewModel> GetCreateViewModelAsync();
        Task<List<SelectListItem>> GetProductSelectListItemsAsync();
        Task<ProductUpdateViewModel> GetUpdateViewModelAsync(int id);
        Task<List<ProductViewModel>> GetProductsAndCategory();
        Task UpdateAsync(ProductViewModel product);
        Task<bool> ToggleActiveAsync(int id);
    }
}