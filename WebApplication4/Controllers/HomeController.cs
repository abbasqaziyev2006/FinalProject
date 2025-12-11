using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.MVC.Models;
using ECommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace EcommerceCoza.MVC.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly IHomeService _homeService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public HomeController(IHomeService homeService, IEmailService emailService, IConfiguration configuration)
        {
            _homeService = homeService;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _homeService.GetHomeViewModelAsync();

            return View(model);
        }

        [Route("About")]
        [HttpGet]
        public IActionResult About()
        {
            return View();
        }

        [Route("Contact")]
        [HttpGet]
        public IActionResult Contact()
        {
            return View();
        }

        [Route("Contact")]
        [HttpPost]
        public async Task<IActionResult> Contact(ContactViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Get admin email from configuration (abbasqaziyev119@gmail.com)
                var adminEmail = _configuration["EmailSettings:AdminEmail"];

                if (string.IsNullOrEmpty(adminEmail))
                {
                    ModelState.AddModelError(string.Empty, "Email configuration is missing. Please contact the administrator.");
                    return View(model);
                }

                var subject = $"New Contact Form Submission from {model.Name}";
                var body = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif;'>
                        <h2 style='color: #333;'>New Contact Form Submission</h2>
                        <div style='background-color: #f5f5f5; padding: 20px; border-radius: 5px;'>
                            <p><strong>Name:</strong> {model.Name}</p>
                            <p><strong>Phone:</strong> {model.Phone}</p>
                            <p><strong>Email:</strong> <a href='mailto:{model.Email}'>{model.Email}</a></p>
                            <hr style='border: 1px solid #ddd;' />
                            <p><strong>Message:</strong></p>
                            <p style='white-space: pre-wrap;'>{model.Comment}</p>
                        </div>
                        <br/>
                        <p style='color: #888; font-size: 12px;'>This email was sent from the contact form on your website.</p>
                        <p style='color: #888; font-size: 12px;'>Reply directly to <a href='mailto:{model.Email}'>{model.Email}</a> to respond to the customer.</p>
                    </body>
                    </html>
                ";

                // Send email to admin (abbasqaziyev119@gmail.com) using Admin credentials
                var emailSent = await _emailService.SendEmailAsync(adminEmail, subject, body, "Admin");

                if (emailSent)
                {
                    ViewData["SuccessMessage"] = "Thank you! Your message has been sent successfully. We will get back to you soon.";
                    ModelState.Clear();
                    return View(new ContactViewModel());
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "An error occurred while sending your message. Please check the email configuration.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
                return View(model);
            }
        }
    }
}