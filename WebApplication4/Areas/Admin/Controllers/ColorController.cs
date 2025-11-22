using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ColorController : Controller
    {
        private readonly IColorService _colorService;

        public ColorController(IColorService colorService)
        {
            _colorService = colorService;
        }

        public async Task<IActionResult> Index()
        {
            var colors = await _colorService.GetAllAsync();
            return View(colors);
        }

        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(ColorCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            await _colorService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int id)
        {
            var model = await _colorService.GetColorUpdateViewModelAsync(id);
            if (model == null) return NotFound();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, ColorUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await _colorService.GetColorUpdateViewModelAsync(id);
                return View(model);
            }

            var isUpdated = await _colorService.UpdateAsync(id, model);
            if (!isUpdated) return NotFound();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var isDeleted = await _colorService.DeleteAsync(id);
            if (!isDeleted) return NotFound();
            return RedirectToAction(nameof(Index));
        }
    }
}