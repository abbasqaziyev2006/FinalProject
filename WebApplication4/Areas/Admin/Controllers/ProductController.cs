using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    public class ProductController : AdminController
    {
        private readonly IProductService _productService;


        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetProductsAndCategory();

            return View(products);
        }

        public async Task<IActionResult> Create()
        {
            var model = await _productService.GetCreateViewModelAsync();

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _productService.GetCreateViewModelAsync();

                return View(model);
            }

            await _productService.CreateAsync(model);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            var model = await _productService.GetUpdateViewModelAsync(id);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, ProductUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _productService.GetUpdateViewModelAsync(id);
                return View(model);
            }

            var isUpdated = await _productService.UpdateAsync(id, model);
            if (!isUpdated)
                return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _productService.DeleteAsync(id);
            if (!isDeleted)
                return NotFound();

            return NoContent();
        }
    }
}
