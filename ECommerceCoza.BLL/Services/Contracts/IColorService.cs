using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IColorService : ICrudService<Color, ColorViewModel, ColorCreateViewModel, ColorUpdateViewModel>
    {
        Task<List<SelectListItem>> GetColorSelectListItemsAsync();
        Task<ColorUpdateViewModel> GetColorUpdateViewModelAsync(int id);
    }
}
