using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services
{
    public class BioManager :CrudManager<Bio, BioViewModel, BioCreateViewModel, BioUpdateViewModel>,
        IBioService
    {
        public BioManager(IRepository<Bio> repository, IMapper mapper)
            :base(repository, mapper) 
        {
            
        }
    }
}
