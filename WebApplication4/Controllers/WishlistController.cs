using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistItemService _wishlistItemService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IProductService _productService;

        public WishlistController(IWishlistItemService wishlistItemService, IProductService productService, UserManager<AppUser> userManager)
        {
            _wishlistItemService = wishlistItemService;
            _productService = productService;
            _userManager = userManager;
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var items = await _wishlistItemService.GetWishlist();

            return View(items);
        }

        [Authorize]
        public async Task<IActionResult> WishlistHeader()
        {
            var items = await _wishlistItemService.GetWishlist();

            return View(items);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Add(int id)
        {
            var productViewModel = await _productService.GetByIdAsync(id);
            if (productViewModel == null)
                return NotFound();

            var username = User.Identity!.Name ?? "";

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return BadRequest();

            var wishlistItems = await _wishlistItemService.GetWishlist();

            var currentItem = wishlistItems.Items.FirstOrDefault(predicate: x=> x.ProductId==id);

            if (currentItem != null)
                return NoContent();


            var createViewModel = new WishlistItemCreateViewModel
            {
                AppUserId = user.Id,
                ProductId = id,
            };

            await _wishlistItemService.CreateAsync(createViewModel);

            return NoContent();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Remove(int id)
        {
            var items = await _wishlistItemService.GetWishlist();
            var product = await _productService.GetByIdAsync(id);
            var currentItem = items.Items.FirstOrDefault(x => x.ProductId == id);
            if (currentItem == null)
                return BadRequest();


            var removed = await _wishlistItemService.DeleteAsync(currentItem.Id);

            if (removed)
            {
                product!.IsInWishlist = false;
                return NoContent();   
            }
            else
                return RedirectToAction("Index");
        }

        public async Task<IActionResult> GetWishlistJ()
        {
            var model = await _wishlistItemService.GetWishlist();

            return Json(model);
        }

    }
}
