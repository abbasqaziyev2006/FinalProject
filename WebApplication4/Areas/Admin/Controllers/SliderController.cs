using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        private readonly ISliderService _sliderService;

        public SliderController(ISliderService sliderService)
        {
            _sliderService = sliderService;
        }

        // GET: Admin/Slider/Index
        public async Task<IActionResult> Index()
        {
            var sliders = await _sliderService.GetAllAsync(
                predicate: s => !s.IsDeleted,
                orderBy: q => q.OrderByDescending(s => s.Id));

            return View(sliders.ToList());
        }

        // GET: Admin/Slider/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Slider/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                await _sliderService.CreateAsync(model);
                TempData["SuccessMessage"] = "Slide created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating slide: {ex.Message}");
                return View(model);
            }
        }

        // GET: Admin/Slider/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var model = await _sliderService.GetSliderUpdateViewModelAsync(id);

            if (model == null)
                return NotFound();

            return View(model);
        }

        // POST: Admin/Slider/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SliderUpdateViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var isUpdated = await _sliderService.UpdateAsync(id, model);

                if (!isUpdated)
                    return NotFound();

                TempData["SuccessMessage"] = "Slide updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating slide: {ex.Message}");
                return View(model);
            }
        }

        // POST: Admin/Slider/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var isDeleted = await _sliderService.DeleteAsync(id);

                if (!isDeleted)
                {
                    TempData["ErrorMessage"] = "Slide not found.";
                    return NotFound();
                }

                TempData["SuccessMessage"] = "Slide deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error deleting slide: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}


