using WebApplication4.Models;
using Microsoft.AspNetCore.Http;

namespace WebApplication4.Services
{
    public interface ICurrencyService
    {
        Currency GetCurrentCurrency();
        void SetCurrency(Currency currency, HttpContext httpContext);
        decimal ConvertFromBaseUsd(decimal amount);
        decimal ConvertBetweenCurrencies(decimal amount, Currency from, Currency to);
        string Format(decimal amount, bool withSymbol = true);
        string GetSymbol(Currency? currency = null);
        string GetCurrencyName(Currency? currency = null);
        decimal GetExchangeRate(Currency currency);
        IEnumerable<(Currency Currency, string Name, string Symbol)> GetAllCurrencies();
    }
}