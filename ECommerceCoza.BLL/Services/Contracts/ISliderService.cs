using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface ISliderService : ICrudService<Slider, SliderViewModel, SliderCreateViewModel, SliderUpdateViewModel>
    {
        Task<SliderUpdateViewModel> GetSliderUpdateViewModelAsync(int id);
    }
}

