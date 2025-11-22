using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;

namespace EcommerceCoza.BLL.Services.Contracts
{
    public interface IWishlistItemService
        : ICrudService<WishlistItem, WishlistItemViewModel, WishlistItemCreateViewModel, WishlistItemUpdateViewModel>
    {
        Task<WishlistItemsViewModel> GetWishlist();
    }
}
