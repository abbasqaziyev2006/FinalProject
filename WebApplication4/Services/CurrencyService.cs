using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using WebApplication4.Models;

namespace WebApplication4.Services
{
    public class CurrencyService : ICurrencyService
    {
        private const string CookieName = "CurrentCurrency";
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Base currency assumed USD stored in DB/prices.
        // Rates can be moved to configuration later.
        private readonly Dictionary<Currency, decimal> _rates = new()
        {
            { Currency.USD, 1m },
            { Currency.AZN, 1.70m }, // 1 USD = 1.70 AZN (example)
            { Currency.RUB, 95m }    // 1 USD = 95 RUB (example)
        };

        private readonly Dictionary<Currency, string> _symbols = new()
        {
            { Currency.USD, "$" },
            { Currency.AZN, "₼" },
            { Currency.RUB, "₽" }
        };

        public CurrencyService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Currency GetCurrentCurrency()
        {
            var ctx = _httpContextAccessor.HttpContext;
            var code = ctx?.Request.Cookies[CookieName];

            if (Enum.TryParse(code, true, out Currency c))
                return c;

            return Currency.USD;
        }

        public void SetCurrency(Currency currency, HttpContext httpContext)
        {
            httpContext.Response.Cookies.Append(CookieName, currency.ToString(), new CookieOptions
            {
                HttpOnly = false,
                IsEssential = true,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }

        public decimal ConvertFromBaseUsd(decimal amount)
        {
            var current = GetCurrentCurrency();
            var rate = _rates[current];
            return decimal.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
        }

        public string Format(decimal amount, bool withSymbol = true)
        {
            var converted = ConvertFromBaseUsd(amount);
            var symbol = withSymbol ? GetSymbol() : "";
            return $"{symbol}{converted:0.##}";
        }

        public string GetSymbol(Currency? currency = null)
        {
            currency ??= GetCurrentCurrency();
            return _symbols[currency.Value];
        }
    }
}