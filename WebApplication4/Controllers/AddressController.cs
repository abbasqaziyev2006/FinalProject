using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;

namespace WebApplication4.Controllers
{
    [Authorize]
    public class AddressController : Controller
    {
        private readonly IAddressService _addressService;

        public AddressController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var addresses = await _addressService.GetAllAsync(a => a.AppUserId == userId, AsNoTracking: true);
            return View(addresses.ToList());
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new AddressCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddressCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            model.AppUserId = userId;

            await _addressService.CreateAddressAsync(model);
            return RedirectToAction(nameof(Index));
        }

        // EDIT: load existing address
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var vm = await _addressService.GetByIdAsync(id);
            if (vm is null || vm.AppUserId != userId)
                return NotFound();

            var updateVm = new AddressUpdateViewModel
            {
                Id = vm.Id,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Company = vm.Company,
                Adress = vm.Adress,
                City = vm.City,
                Country = vm.Country,
                PostalCode = vm.PostalCode,
                Phone = vm.Phone,
                AppUserId = vm.AppUserId,
                IsDefault = vm.IsDefault
            };

            return View(updateVm);
        }

        // EDIT: save changes
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AddressUpdateViewModel model)
        {
            if (id != model.Id)
                return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // ensure the address belongs to the current user
            var existing = await _addressService.GetByIdAsync(id);
            if (existing is null || existing.AppUserId != userId)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            model.AppUserId = userId; // enforce ownership
            var ok = await _addressService.UpdateAsync(id, model);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, "Unable to update address.");
                return View(model);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}