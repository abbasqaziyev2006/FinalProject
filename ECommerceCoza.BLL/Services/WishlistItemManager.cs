using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;

namespace EcommerceCoza.BLL.Services
{
    public class WishlistItemManager: 
        CrudManager<WishlistItem, WishlistItemViewModel, WishlistItemCreateViewModel, WishlistItemUpdateViewModel>,
        IWishlistItemService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public WishlistItemManager(IRepository<WishlistItem> repository, IMapper mapper, IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager)
            : base(repository, mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<WishlistItemsViewModel> GetWishlist()
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            var itemsT = new WishlistItemsViewModel();

            if (currentUser != null && currentUser.Identity!.IsAuthenticated)
            {
                string userName = currentUser.Identity.Name!;

                var items = await base.GetAllAsync(predicate: x => x.AppUser.UserName == userName && !x.IsDeleted,
                  include: x => x
                  .Include(p => p.Product).ThenInclude(pv => pv.ProductVariants).ThenInclude(c => c.Color!)
                  .Include(p => p.Product).ThenInclude(pv => pv.ProductVariants).ThenInclude(i => i.ProductImages));

                foreach (var item in items)
                {
                    item.Product!.IsInWishlist = true;
                }

                itemsT.Items = items.ToList();
                itemsT.Count = items.ToList().Count;
            }

            return itemsT;
        }
        


    }
}
