using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : AdminController
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

      
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllAsync(
                include: x => x.Include(c => c.Products));

            return View(categories.ToList());
        }

   
        public IActionResult Create()
        {
            return View();
        }

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _categoryService.CreateAsync(model);
                TempData["SuccessMessage"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating category: {ex.Message}");
                return View(model);
            }
        }

     
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _categoryService.GetCategoryUpdateViewModelAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoryUpdateViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var isUpdated = await _categoryService.UpdateAsync(id, model);

                if (!isUpdated)
                    return NotFound();

                TempData["SuccessMessage"] = "Category updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating category: {ex.Message}");
                return View(model);
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var isDeleted = await _categoryService.DeleteAsync(id);

                if (!isDeleted)
                {
                    TempData["ErrorMessage"] = "Category not found.";
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Category deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting category: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}