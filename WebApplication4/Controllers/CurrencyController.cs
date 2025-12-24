using Microsoft.AspNetCore.Mvc;
using WebApplication4.Services;
using WebApplication4.Models;

namespace WebApplication4.Controllers
{
    public class CurrencyController : Controller
    {
        private readonly ICurrencyService _currencyService;
        public CurrencyController(ICurrencyService currencyService) => _currencyService = currencyService;

        [HttpPost]
        public IActionResult SetCurrency(string currency, string? returnUrl)
        {
            if (Enum.TryParse<Currency>(currency, true, out var c))
            {
                _currencyService.SetCurrency(c, HttpContext);
            }

            if (string.IsNullOrWhiteSpace(returnUrl))
                returnUrl = Url.Action("Index", "Home") ?? "/";

            return LocalRedirect(returnUrl);
        }
    }
}