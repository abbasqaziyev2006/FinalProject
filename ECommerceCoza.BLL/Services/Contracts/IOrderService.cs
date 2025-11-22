using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;


namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IOrderService 
        : ICrudService<Order, OrderViewModel, OrderCreateViewModel, OrderUpdateViewModel>
    {
        Task<OrderCreateViewModel> GetUserAndAddressViewModel(OrderCreateViewModel model);
        
        Task<DiscountCodeViewModel> GetDiscount(string discountCode);

        Task<List<OrderViewModel>> GetOrderViewModelsAsync(string userId);
        Task<OrderViewModel> GetDetailsOfOrderAsync(int orderId);
    }
}
