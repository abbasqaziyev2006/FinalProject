using WebApplication4.Models;
using Microsoft.AspNetCore.Http;

namespace WebApplication4.Services
{
    public interface ICurrencyService
    {
        Currency GetCurrentCurrency();
        void SetCurrency(Currency currency, HttpContext httpContext);
        decimal ConvertFromBaseUsd(decimal amount);
        string Format(decimal amount, bool withSymbol = true);
        string GetSymbol(Currency? currency = null);
    }
}