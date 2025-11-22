using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services
{
    public class DiscountCodeManager : CrudManager<DiscountCode, DiscountCodeViewModel, DiscountCodeCreateViewModel, DiscountCodeUpdateViewModel>,
    IDiscountCodeService
    {
        public DiscountCodeManager(IRepository<DiscountCode> repository, IMapper mapper) : base(repository, mapper)
        {
        }

    }
}
