using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EcommerceCoza.BLL.Services
{
    public class ColorManager : CrudManager<Color, ColorViewModel, ColorCreateViewModel, ColorUpdateViewModel>,
      IColorService
    {
        public ColorManager(IRepository<Color> repository, IMapper mapper)
            : base(repository, mapper)
        {

        }

        public async Task<ColorUpdateViewModel> GetColorUpdateViewModelAsync(int id)
        {
            var color = await Repository.GetByIdAsync(id);

            if (color == null)
                return null!;

            var colorUpdateViewModel = Mapper.Map<ColorUpdateViewModel>(color);

            return colorUpdateViewModel;
        }

        public async Task<List<SelectListItem>> GetColorSelectListItemsAsync()
        {
            var colors = await GetAllAsync(predicate: x => !x.IsDeleted);

            return colors.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
            }).ToList();
        }
    }
}
