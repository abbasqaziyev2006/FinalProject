using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface ISocialService:ICrudService<Social, SocialViewModel, SocialCreateViewModel, SocialUpdateViewModel> { }
}
