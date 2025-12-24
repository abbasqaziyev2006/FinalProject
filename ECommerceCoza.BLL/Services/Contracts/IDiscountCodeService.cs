using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;


namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IDiscountCodeService
      : ICrudService<DiscountCode, DiscountCodeViewModel, DiscountCodeCreateViewModel, DiscountCodeUpdateViewModel>
    {

    }
}
