using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BrandController : Controller
    {
        private readonly IBrandService _brandService;

        public BrandController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            var brands = await _brandService.GetBrandsWithPaginationAsync(pageNumber, pageSize);
            return View(brands);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (await _brandService.ExistsByNameAsync(model.Name))
            {
                ModelState.AddModelError("Name", "Brand name already exists.");
                return View(model);
            }

            await _brandService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand == null) return NotFound();

            var updateModel = new BrandUpdateViewModel
            {
                Id = brand.Id,
                Name = brand.Name,
                Description = brand.Description,
                CurrentLogoUrl = brand.LogoUrl,
                IsActive = brand.IsActive
            };

            return View(updateModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BrandUpdateViewModel model)
        {
            if (id != model.Id) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            if (await _brandService.ExistsByNameAsync(model.Name, id))
            {
                ModelState.AddModelError("Name", "Brand name already exists.");
                return View(model);
            }

            var isUpdated = await _brandService.UpdateAsync(id, model);
            if (!isUpdated) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _brandService.DeleteAsync(id);
            if (!isDeleted) return NotFound();
            return RedirectToAction(nameof(Index));
        }
    }
}