using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.MVC.Models;
using ECommerceCoza.BLL.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Controllers
{
    public partial class HomeController : Controller
    {
        private readonly IHomeService _homeService;
        private readonly IEmailService _emailService;

        public HomeController(IHomeService homeService, IEmailService emailService)
        {
            _homeService = homeService;
            _emailService = emailService;
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
                var subject = $"New Contact Form Submission from {model.Name}";
                var body = $@"
            <h3>New Contact Form Submission</h3>
            <p><strong>Name:</strong> {model.Name}</p>
            <p><strong>Phone:</strong> {model.Phone}</p>
            <p><strong>Email:</strong> {model.Email}</p>
            <p><strong>Message:</strong></p>
            <p>{model.Comment}</p>
        ";

                await _emailService.SendEmailAsync(model.Email, subject, body);

                ViewData["SuccessMessage"] = "Thank you! Your message has been sent successfully.";
                return View(new ContactViewModel());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred while sending your message. Please try again.");
                return View(model);
            }
        }

    }
}
