using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IOrderDetailService
        : ICrudService<OrderDetail, OrderDetailViewModel, OrderDetailCreateViewModel, OrderDetailUpdateViewModel>
    {
        Task<List<OrderDetailCreateViewModel>> GetOrderDetailCreateViewModels();
    }
}
