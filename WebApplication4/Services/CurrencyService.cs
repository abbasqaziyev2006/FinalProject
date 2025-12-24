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

        private readonly Dictionary<Currency, decimal> _rates = new()
        {
            { Currency.USD, 1m },
            { Currency.AZN, 1.70m },
            { Currency.RUB, 95m }
        };

        private readonly Dictionary<Currency, string> _symbols = new()
        {
            { Currency.USD, "$" },
            { Currency.AZN, "₼" },
            { Currency.RUB, "₽" }
        };

        private readonly Dictionary<Currency, string> _names = new()
        {
            { Currency.USD, "US Dollar" },
            { Currency.AZN, "Azerbaijani Manat" },
            { Currency.RUB, "Russian Ruble" }
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

        public decimal ConvertBetweenCurrencies(decimal amount, Currency from, Currency to)
        {
            if (from == to)
                return amount;

            var fromRate = _rates[from];
            var toRate = _rates[to];
            var amountInUsd = amount / fromRate;
            var convertedAmount = amountInUsd * toRate;

            return decimal.Round(convertedAmount, 2, MidpointRounding.AwayFromZero);
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

        public string GetCurrencyName(Currency? currency = null)
        {
            currency ??= GetCurrentCurrency();
            return _names[currency.Value];
        }

        public decimal GetExchangeRate(Currency currency)
        {
            return _rates[currency];
        }

        public IEnumerable<(Currency Currency, string Name, string Symbol)> GetAllCurrencies()
        {
            foreach (Currency currency in Enum.GetValues(typeof(Currency)))
            {
                yield return (currency, _names[currency], _symbols[currency]);
            }
        }
    }
}