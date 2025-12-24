using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceCoza.MVC.Controllers
{
    public class LocalizationController : Controller
    {
        [HttpPost]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(culture))
            {
                Response.Cookies.Append(
                    CookieRequestCultureProvider.DefaultCookieName,
                    CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                    new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });
            }

            return LocalRedirect(string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl);
        }
    }
}