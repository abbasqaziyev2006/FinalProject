using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DiscountCodeController : Controller
    {
        private readonly IDiscountCodeService _discountCodeService;

        public DiscountCodeController(IDiscountCodeService discountCodeService)
        {
            _discountCodeService = discountCodeService;
        }

        public async Task<IActionResult> Index()
        {
            var discountCodes = await _discountCodeService.GetAllAsync();
            return View(discountCodes);
        }

        public IActionResult Create()
        {
            return View("~/Areas/Admin/Views/DiscountCode/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiscountCodeCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _discountCodeService.CreateAsync(model);
                TempData["Success"] = "Discount code created successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error creating discount code: {ex.Message}");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            var discountCode = await _discountCodeService.GetByIdAsync(id);
            if (discountCode == null)
            {
                TempData["Error"] = "Discount code not found.";
                return NotFound();
            }

            var updateModel = new DiscountCodeUpdateViewModel
            {
                Id = discountCode.Id,
                Code = discountCode.Code,
                SalePercentage = discountCode.SalePercentage,
                IsActive = discountCode.IsActive
            };

            return View(updateModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DiscountCodeUpdateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                await _discountCodeService.UpdateAsync(model.Id, model);
                TempData["Success"] = "Discount code updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error updating discount code: {ex.Message}");
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _discountCodeService.DeleteAsync(id);
                TempData["Success"] = "Discount code deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting discount code: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}