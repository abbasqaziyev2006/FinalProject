using AutoMapper;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;
using System.Security.Claims;

namespace EcommerceCoza.BLL.Services
{
    public class ProductManager : CrudManager<Product, ProductViewModel, ProductCreateViewModel, ProductUpdateViewModel>,
        IProductService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWishlistItemService _wishlistService;
        private readonly ICategoryService _categoryService;

        public ProductManager(IRepository<Product> repository, IMapper mapper, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, IWishlistItemService wishlistService, ICategoryService categoryService)
            : base(repository, mapper)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _wishlistService = wishlistService;
            _categoryService = categoryService;
        }

        public override async Task<IEnumerable<ProductViewModel>> GetAllAsync(Expression<Func<Product, bool>>? predicate = null, Func<IQueryable<Product>, IIncludableQueryable<Product, object>>? include = null, Func<IQueryable<Product>,
            IOrderedQueryable<Product>>? orderBy = null, bool AsNoTracking = false)
        {
            var currentUser = _httpContextAccessor.HttpContext?.User;
            List<WishlistItemViewModel> wishlistItems = [];

            var products = await base.GetAllAsync();

            if (currentUser != null && currentUser.Identity!.IsAuthenticated)
            {
                var userId = currentUser.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                foreach (var product in products)
                {
                    var item = await _wishlistService.GetAsync(
                        predicate: x => x.AppUserId == userId && x.ProductId == product.Id,
                        include: x => x.Include(p => p.Product));

                    if (item != null)
                        product!.IsInWishlist = true;

                    else
                        product.IsInWishlist = false;
                }
            }
            else
            {
                foreach (var product in products)
                {
                    product.IsInWishlist = false;
                }
            }
            
            return await base.GetAllAsync(predicate: x => !x.IsDeleted
              , include: x => x
              .Include(pv => pv.ProductVariants).ThenInclude(i => i.ProductImages)
              .Include(pv => pv.ProductVariants).ThenInclude(c => c.Color!))
               ;
        }

        public async Task<ProductCreateViewModel> GetCreateViewModelAsync()
        {
            var model = new ProductCreateViewModel();
            model.CategorySelectListItems = await _categoryService.GetCategorySelectListItemsAsync();

            return model;
        }

        public async Task<List<SelectListItem>> GetProductSelectListItemsAsync()
        {
            var products = await GetAllAsync(predicate: x => !x.IsDeleted);

            return products.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
            }).ToList();
        }

        public async Task<ProductUpdateViewModel> GetUpdateViewModelAsync(int id)
        {
            var product = await Repository.GetByIdAsync(id);

            if (product == null)
                return null!;

            var model = Mapper.Map<ProductUpdateViewModel>(product);
            model.CategorySelectListItems = await _categoryService.GetCategorySelectListItemsAsync();

            return model;
        }

        public async Task<List<ProductViewModel>> GetProductsAndCategory()
        {
            var products = await GetAllAsync(include: x=>x.Include(c=>c.Category!));
            foreach(var product in products)
            {
                var category = await _categoryService.GetByIdAsync(product.CategoryId);
                if(category != null)
                product.CategoryName = category.Name;
            }

            return products.ToList();
        }
    }
}
