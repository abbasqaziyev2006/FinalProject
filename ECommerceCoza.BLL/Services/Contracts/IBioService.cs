using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IBioService : ICrudService<Bio, BioViewModel, BioCreateViewModel, BioUpdateViewModel> { }
}
