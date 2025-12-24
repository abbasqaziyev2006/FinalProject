using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services
{
    public class SocialManager:CrudManager<Social, SocialViewModel, SocialCreateViewModel, SocialUpdateViewModel>,
        ISocialService
    {
        public SocialManager(IRepository<Social> repository, IMapper mapper)
            :base(repository, mapper) 
        {
            
        }
    }
}
