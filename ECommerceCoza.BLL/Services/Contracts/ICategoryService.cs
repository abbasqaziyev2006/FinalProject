using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface ICategoryService : ICrudService<Category, CategoryViewModel, CategoryCreateViewModel, CategoryUpdateViewModel>
    {
        Task<CategoryUpdateViewModel> GetCategoryUpdateViewModelAsync(int id);
        Task<List<SelectListItem>> GetCategorySelectListItemsAsync();
    }
}
