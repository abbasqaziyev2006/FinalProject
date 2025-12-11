using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.MVC.Models;
using ECommerceCoza.BLL.Services.Contracts;
using ECommerceCoza.DAL.DataContext.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

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
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

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

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email or password is incorrect.");

                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", $"You are banned {user.LockoutEnd.Value.AddHours(4).ToString()}.");

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

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Don't reveal that the user does not exist
            if (user == null)
            {
                TempData["ForgotPasswordStatus"] = "If the email exists in our system, a password reset link has been sent.";
                return View();
            }

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
                <h2>Password Reset</h2>
                <p>You requested a password reset. Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you didn't request this, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>";

            var emailSent = await _emailService.SendEmailAsync(user.Email!, subject, message, "User");

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

            // Check if new password is the same as current password
            var isSamePassword = await _userManager.CheckPasswordAsync(user, model.NewPassword);
            if (isSamePassword)
            {
                ModelState.AddModelError("NewPassword", "New password cannot be the same as your current password.");
                return View(model);
            }

            // Decode the token
            var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(model.Token));

            // Reset the password
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