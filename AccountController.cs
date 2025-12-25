using EcommerceCoza.BLL.Services;
using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.MVC.Models;
using ECommerceCoza.BLL.Services.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;

namespace EcommerceCoza.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWishlistItemService _wishlistItemService;
        private readonly IProductService _productService;
        private readonly IEmailService _emailService; // Cleaned up duplicate
        private readonly BasketManager _basketManager;

        public AccountController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IWishlistItemService userWishlistItemService,
            IProductService productService,
            IEmailService emailService,
            BasketManager basketManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _wishlistItemService = userWishlistItemService;
            _productService = productService;
            _emailService = emailService;
            _basketManager = basketManager;
        }

        // OPTIMIZED: This now executes entirely in SQL Server
        private async Task<bool> IsPhoneNumberTaken(string? phoneNumber, string? excludeUserId = null)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return false;

            var normalizedInput = Regex.Replace(phoneNumber, @"\D", "");

            if (string.IsNullOrEmpty(normalizedInput))
                return false;

            var query = _userManager.Users
                .Where(u => u.PhoneNumberNormalized == normalizedInput);

            if (!string.IsNullOrEmpty(excludeUserId))
                query = query.Where(u => u.Id != excludeUserId);

            return await query.AnyAsync();
        }

        [Authorize]
        public async Task<IActionResult> Index()
        {
            var username = User.Identity!.Name ?? "";
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return BadRequest();

            var model = new AccountViewModel
            {
                UserName = user.UserName,
            };

            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Check email uniqueness
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // Check username uniqueness
            var existingUserByName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByName != null)
            {
                ModelState.AddModelError("UserName", "This username is already taken.");
                return View(model);
            }

            // Check phone uniqueness using optimized helper
            if (await IsPhoneNumberTaken(model.PhoneNumber))
            {
                ModelState.AddModelError("PhoneNumber", "This phone number is already registered.");
                return View(model);
            }

            var user = new AppUser
            {
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                // Store normalized version for fast searching
                PhoneNumberNormalized = string.IsNullOrEmpty(model.PhoneNumber) 
                    ? null 
                    : Regex.Replace(model.PhoneNumber, @"\D", "")
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                    ModelState.AddModelError("", item.Description);
                
                return View(model);
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            _basketManager.TransferGuestBasketToUser();

            TempData["SuccessMessage"] = "Your account has been created successfully!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

            if (result.IsLockedOut)
            {
                var lockout = user.LockoutEnd ?? DateTimeOffset.UtcNow;
                var bannedUntilText = lockout == DateTimeOffset.MaxValue 
                    ? "indefinitely" 
                    : lockout.AddHours(4).ToString("yyyy-MM-dd HH:mm");

                ModelState.AddModelError("", $"You are banned until {bannedUntilText}.");
                return View(model);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            _basketManager.TransferGuestBasketToUser();

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var user = await _userManager.FindByNameAsync(User.Identity!.Name!);
            if (user == null) return BadRequest();

            var model = new EditAccountViewModel
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByNameAsync(User.Identity!.Name!);
            if (user == null) return BadRequest();

            // Password change
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                var res = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!res.Succeeded)
                {
                    foreach (var err in res.Errors) ModelState.AddModelError("", err.Description);
                    return View(model);
                }
            }

            // Email & Phone Uniqueness checks
            if (model.Email != user.Email)
            {
                var existing = await _userManager.FindByEmailAsync(model.Email);
                if (existing != null)
                {
                    ModelState.AddModelError("Email", "Email is already in use.");
                    return View(model);
                }
                await _userManager.SetEmailAsync(user, model.Email);
            }

            if (model.PhoneNumber != user.PhoneNumber)
            {
                if (await IsPhoneNumberTaken(model.PhoneNumber, user.Id))
                {
                    ModelState.AddModelError("PhoneNumber", "Phone number is already in use.");
                    return View(model);
                }
                await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber);
                user.PhoneNumberNormalized = string.IsNullOrEmpty(model.PhoneNumber) 
                    ? null 
                    : Regex.Replace(model.PhoneNumber, @"\D", "");
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.UserName = model.UserName;

            var finalResult = await _userManager.UpdateAsync(user);
            if (!finalResult.Succeeded)
            {
                foreach (var err in finalResult.Errors) ModelState.AddModelError("", err.Description);
                return View(model);
            }

            return RedirectToAction(nameof(Edit));
        }

        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var resetLink = Url.Action("ResetPassword", "Account", new { userId = user.Id, token = encodedToken }, Request.Scheme);

                var message = $"Please reset your password by <a href='{resetLink}'>clicking here</a>.";
                await _emailService.SendEmailAsync(user.Email!, "Reset Password", message, "Admin");
            }

            TempData["ForgotPasswordStatus"] = "If the email exists, a reset link has been sent.";
            return View();
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token)) return BadRequest();
            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null) return BadRequest();

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["ResetPasswordSuccess"] = "Password reset successful.";
                return RedirectToAction("Login");
            }

            foreach (var err in result.Errors) ModelState.AddModelError("", err.Description);
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied() => View();
    }
}