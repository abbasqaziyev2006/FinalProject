using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using ECommerceCoza.DAL.DataContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    public class ProductVariantController : AdminController
    {
        private readonly AppDbContext _dbContext;
        private readonly IProductVariantService _productVariantService;

        public ProductVariantController(IProductVariantService productVariantService, AppDbContext dbContext)
        {
            _productVariantService = productVariantService;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Create()
        {
            var model = await _productVariantService.GetCreateViewModelAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductVariantCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _productVariantService.GetCreateViewModelAsync();

                return View(model);
            }

            await _productVariantService.CreateAsync(model);

            return RedirectToAction("Index", "Product");
        }

        public async Task<IActionResult> Update(int id)
        {
            var model = await _productVariantService.GetProductVariantUpdateViewModelAsync(id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, ProductVariantUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _productVariantService.GetProductVariantUpdateViewModelAsync(id);
                return View(model);
            }
            var isUpdated = await _productVariantService.UpdateAsync(id, model);
            if (!isUpdated)
                return NotFound();

            return RedirectToAction("Index", "Product");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProductImage(int id)
        {
            var productImage = await _dbContext.ProductImages
                .FirstOrDefaultAsync(x => x.Id == id);

            if (productImage == null) return NotFound();

            _dbContext.ProductImages.Remove(productImage);
            await _dbContext.SaveChangesAsync();

            return Json(new { IsDeleted = true });
        }
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _productVariantService.DeleteAsync(id);
            if (!isDeleted)
                return NotFound();

            return NoContent();
        }

    }
}
