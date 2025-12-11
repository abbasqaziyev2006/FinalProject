using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.MVC.Models;
using ECommerceCoza.BLL.Services.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace EcommerceCoza.MVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IWishlistItemService _wishlistItemService;
        private readonly IProductService _productService;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager, IWishlistItemService userWishlistItemService, IProductService productService, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _wishlistItemService = userWishlistItemService;
            _productService = productService;
            _emailService = emailService;
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

            // Check if email already exists to prevent duplicates
            var existingUserByEmail = await _userManager.Users
                .Where(u => u.NormalizedEmail == model.Email.ToUpper())
                .FirstOrDefaultAsync();

            if (existingUserByEmail != null)
            {
                ModelState.AddModelError("Email", "This email is already registered.");
                return View(model);
            }

            // Check if username already exists
            var existingUserByName = await _userManager.FindByNameAsync(model.UserName);
            if (existingUserByName != null)
            {
                ModelState.AddModelError("UserName", "This username is already taken.");
                return View(model);
            }

            var user = new AppUser
            {
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }

                return View(model);
            }

            // Automatically sign in the user after registration
            await _signInManager.SignInAsync(user, isPersistent: false);

            TempData["SuccessMessage"] = "Your account has been created successfully!";
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Use direct query to handle potential duplicates gracefully
            var normalizedEmail = model.Email.ToUpper();
            var users = await _userManager.Users
                .Where(u => u.NormalizedEmail == normalizedEmail)
                .ToListAsync();

            if (users.Count == 0)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            if (users.Count > 1)
            {
                // Critical: Multiple accounts with same email
                ModelState.AddModelError("", "Multiple accounts found with this email. Please contact support to resolve this issue.");
                // Log this error for admin attention
                // You can add logging here: _logger.LogError($"Duplicate email found: {model.Email}");
                return View(model);
            }

            var user = users[0];

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", $"You are banned until {user.LockoutEnd.Value.AddHours(4):yyyy-MM-dd HH:mm}.");
                return View(model);
            }

            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");
                return View(model);
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public async Task<IActionResult> Edit()
        {
            var username = User.Identity!.Name ?? "";

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return BadRequest();

            var editAccountViewModel = new EditAccountViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,

            };


            return View(editAccountViewModel);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Edit(EditAccountViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            //find user
            var username = User.Identity!.Name ?? "";

            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return BadRequest();

            //check password
            if (!string.IsNullOrEmpty(model.CurrentPassword) && !string.IsNullOrEmpty(model.NewPassword))
            {
                // Check if new password is the same as current password
                if (model.CurrentPassword == model.NewPassword)
                {
                    ModelState.AddModelError("NewPassword", "New password cannot be the same as the current password.");
                    return View(model);
                }

                var resultPassword = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!resultPassword.Succeeded)
                {
                    foreach (var error in resultPassword.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
            }

            if (model.Email != user.Email /*&& !string.IsNullOrEmpty(model.Email) && !string.IsNullOrEmpty(user.Email)*/)
            {
                // Check if new email already exists
                var existingUser = await _userManager.Users
                    .Where(u => u.NormalizedEmail == model.Email.ToUpper() && u.Id != user.Id)
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "This email is already in use by another account.");
                    return View(model);
                }

                var resultEmail = await _userManager.SetEmailAsync(user, model.Email);
                if (!resultEmail.Succeeded)
                {
                    foreach (var error in resultEmail.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }

                    return View(model);
                }
            }

            if (model.FirstName != user.FirstName)
                user.FirstName = model.FirstName;

            if (model.LastName != user.LastName)
                user.LastName = model.LastName;

            var resultTotal = await _userManager.UpdateAsync(user);

            if (!resultTotal.Succeeded)
            {
                foreach (var error in resultTotal.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return RedirectToAction(nameof(Index));

        }

        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Use direct query to handle potential duplicates
            var normalizedEmail = model.Email.ToUpper();
            var users = await _userManager.Users
                .Where(u => u.NormalizedEmail == normalizedEmail)
                .ToListAsync();

            // Don't reveal that the user does not exist or that there are duplicates
            if (users.Count == 0 || users.Count > 1)
            {
                TempData["ForgotPasswordStatus"] = "If the email exists in our system, a password reset link has been sent.";
                return View();
            }

            var user = users[0];

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode the token for URL
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Create reset link
            var resetLink = Url.Action("ResetPassword", "Account",
                new { userId = user.Id, token = encodedToken },
                Request.Scheme);

            // Send email
            var subject = "Password Reset Request";
            var message = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                    <h2 style='color: #333;'>Password Reset</h2>
                    <p>Hello {user.FirstName ?? "User"},</p>
                    <p>You requested a password reset. Click the button below to reset your password:</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #007bff; color: white; padding: 12px 30px; text-decoration: none; border-radius: 5px; display: inline-block;'>Reset Password</a>
                    </div>
                    <p>Or copy and paste this link into your browser:</p>
                    <p style='word-break: break-all; color: #666;'>{resetLink}</p>
                    <p>If you didn't request this, please ignore this email.</p>
                    <p style='color: #999; font-size: 12px;'>This link will expire in 24 hours.</p>
                    <hr style='border: none; border-top: 1px solid #ddd; margin: 20px 0;'>
                    <p style='color: #999; font-size: 11px;'>This is an automated message from your E-Commerce Store. Please do not reply to this email.</p>
                </div>";


            var emailSent = await _emailService.SendEmailAsync(user.Email!, subject, message, "Admin");

            if (emailSent)
            {
                TempData["ForgotPasswordStatus"] = "A password reset link has been sent to your email.";
            }
            else
            {
                TempData["ForgotPasswordStatus"] = "There was an error sending the email. Please try again later.";
            }

            return View();
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return BadRequest("Invalid password reset link.");
            }

            var model = new ResetPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid user.");
                return View(model);
            }
            var isSamePassword = await _userManager.CheckPasswordAsync(user, model.NewPassword);
            if (isSamePassword)
            {
                ModelState.AddModelError("NewPassword", "New password cannot be the same as your current password.");
                return View(model);
            }

            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }

            TempData["ResetPasswordSuccess"] = "Your password has been reset successfully. Please login with your new password.";
            return RedirectToAction("Login");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}