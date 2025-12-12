using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 100)
        {
            var brands = await _brandService.GetAllAsync(
                predicate: b => !b.IsDeleted,
                include: q => q.Include(b => b.Products!)
            );

            return View(brands.ToList());
        }

        public IActionResult Create()
        {
            var model = new BrandCreateViewModel { IsActive = true };
            return View(model);
        }

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

            try
            {
                await _brandService.CreateAsync(model);
                TempData["SuccessMessage"] = $"Brand '{model.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating brand: {ex.Message}");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var brand = await _brandService.GetByIdAsync(id);
            if (brand == null)
            {
                TempData["ErrorMessage"] = "Brand not found.";
                return NotFound();
            }

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

            try
            {
                var isUpdated = await _brandService.UpdateAsync(id, model);
                if (!isUpdated)
                {
                    TempData["ErrorMessage"] = "Brand not found.";
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Brand '{model.Name}' updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating brand: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var brand = await _brandService.GetByIdAsync(id);
                var brandName = brand?.Name ?? "Unknown";

                var isDeleted = await _brandService.DeleteAsync(id);
                if (!isDeleted)
                {
                    TempData["ErrorMessage"] = "Brand not found.";
                    return NotFound();
                }

                TempData["SuccessMessage"] = $"Brand '{brandName}' deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting brand: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}