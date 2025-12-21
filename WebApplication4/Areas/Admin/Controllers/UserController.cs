using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using EcommerceCoza.MVC.Models;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.MVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IOrderService _orderService;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<AppUser> userManager, IOrderService orderService, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _orderService = orderService;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchTerm = "")
        {
            IEnumerable<AppUser> users;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                users = await _userManager.Users
                    .Where(u => u.UserName!.Contains(searchTerm) ||
                               u.Email!.Contains(searchTerm) ||
                               u.FirstName!.Contains(searchTerm) ||
                               u.LastName!.Contains(searchTerm))
                    .ToListAsync();
            }
            else
            {
                users = await _userManager.Users.ToListAsync();
            }

            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                // Get total orders for the user
                var userOrders = await _orderService.GetOrderViewModelsAsync(user.Id);

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    EmailConfirmed = user.EmailConfirmed,
                    TotalOrders = userOrders.Count,
                    IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.Now
                });
            }

            var model = new UserListViewModel
            {
                Users = userViewModels,
                SearchTerm = searchTerm
            };

            return View("~/Areas/Admin/Views/User/Index.cshtml", model);
        }

        // GET: Admin/User/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var allRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.Now,
                AllRoles = allRoles,
                SelectedRoles = userRoles.ToList()
            };

            return View("~/Areas/Admin/Views/User/Edit.cshtml", model);
        }

        // POST: Admin/User/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (model == null)
                return BadRequest();

            // Ensure roles list is available if we need to redisplay the form
            model.AllRoles = model.AllRoles ?? (await _roleManager.Roles.Select(r => r.Name!).ToListAsync());

            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/User/Edit.cshtml", model);

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            // Email change: use SetEmailAsync to keep identity metadata consistent
            if (!string.Equals(user.Email, model.Email, StringComparison.OrdinalIgnoreCase))
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View("~/Areas/Admin/Views/User/Edit.cshtml", model);
                }
            }

            // Update basic properties
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.EmailConfirmed = model.EmailConfirmed;

            // Handle IsActive by toggling lockout end (simple approach)
            if (model.IsActive)
            {
                user.LockoutEnd = null;
                user.LockoutEnabled = false;
            }
            else
            {
                user.LockoutEnabled = true;
                user.LockoutEnd = DateTimeOffset.MaxValue;
            }

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError("", error.Description);

                return View("~/Areas/Admin/Views/User/Edit.cshtml", model);
            }

            // Roles management
            var currentRoles = (await _userManager.GetRolesAsync(user)).ToList();
            var selectedRoles = model.SelectedRoles ?? new List<string>();

            var rolesToAdd = selectedRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(selectedRoles).ToList();

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    foreach (var error in addResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View("~/Areas/Admin/Views/User/Edit.cshtml", model);
                }
            }

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                        ModelState.AddModelError("", error.Description);

                    return View("~/Areas/Admin/Views/User/Edit.cshtml", model);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}