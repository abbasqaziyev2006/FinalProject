using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
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
               
                var userOrders = await _orderService.GetOrderViewModelsAsync(user.Id);

             
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

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
                    IsActive = !user.LockoutEnabled || user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.Now,
                    IsAdmin = isAdmin
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

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (model == null)
                return BadRequest();

            model.AllRoles = model.AllRoles ?? (await _roleManager.Roles.Select(r => r.Name!).ToListAsync());

            if (!ModelState.IsValid)
                return View("~/Areas/Admin/Views/User/Edit.cshtml", model);

            var user = await _userManager.FindByIdAsync(model.Id!);
            if (user == null)
                return NotFound();

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

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.PhoneNumber = model.PhoneNumber;
            user.EmailConfirmed = model.EmailConfirmed;

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

            // Roles management via SelectedRoles
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return NotFound();
            }

            if (user.UserName == User.Identity?.Name)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User '{user.UserName}' deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Error deleting user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Index));
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PromoteToAdmin(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (user.UserName == User.Identity?.Name)
            {
                TempData["ErrorMessage"] = "You cannot change your own role here.";
                return RedirectToAction(nameof(Index));
            }

            const string adminRole = "Admin";
            if (!await _roleManager.RoleExistsAsync(adminRole))
            {
                var createRoleResult = await _roleManager.CreateAsync(new IdentityRole(adminRole));
                if (!createRoleResult.Succeeded)
                {
                    TempData["ErrorMessage"] = "Failed to create Admin role.";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (await _userManager.IsInRoleAsync(user, adminRole))
            {
                TempData["ErrorMessage"] = "User is already an admin.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.AddToRoleAsync(user, adminRole);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User '{user.UserName}' promoted to Admin.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to promote user: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Index));
        }

    
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RevokeAdmin(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

           
            if (user.UserName == User.Identity?.Name)
            {
                TempData["ErrorMessage"] = "You cannot revoke your own admin role here.";
                return RedirectToAction(nameof(Index));
            }

            const string adminRole = "Admin";

            if (!await _roleManager.RoleExistsAsync(adminRole))
            {
                TempData["ErrorMessage"] = "Admin role does not exist.";
                return RedirectToAction(nameof(Index));
            }

            if (!await _userManager.IsInRoleAsync(user, adminRole))
            {
                TempData["ErrorMessage"] = "User is not an admin.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _userManager.RemoveFromRoleAsync(user, adminRole);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Admin role revoked from '{user.UserName}'.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to revoke admin role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}