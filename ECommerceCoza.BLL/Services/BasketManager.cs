using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.Services
{
    public class BasketManager
    {
        private const string BasketCookiePrefix = "basket_";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly IProductVariantService _productVariantService;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        public BasketManager(IProductService productService, IHttpContextAccessor httpContextAccessor, IProductVariantService productVariantService)
        {
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
            _productVariantService = productVariantService;
        }

        private string GetBasketCookieName()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
                return $"{BasketCookiePrefix}{userId}";

            return $"{BasketCookiePrefix}guest";
        }

        public async Task<BasketViewModel> GetBasketAsync()
        {
            var basket = GetBasketFromCookie();
            return await BuildBasketViewModelAsync(basket);
        }

        public async Task<BasketViewModel> ChangeQuantityAsync(int productVariantId, int quantity)
        {
            var basket = GetBasketFromCookie();
            var basketItem = basket.FirstOrDefault(item => item.ProductVariantId == productVariantId);

            if (basketItem != null)
            {
                basketItem.Quantity += quantity;

                if (basketItem.Quantity <= 0)
                    basket.Remove(basketItem);

                SaveBasketToCookie(basket);
            }

            return await BuildBasketViewModelAsync(basket);
        }

        // Keep synchronous AddToBasket for backward compatibility
        public void AddToBasket(int productVariantId, int quantity)
        {
            var basket = GetBasketFromCookie();
            var basketItem = basket.FirstOrDefault(item => item.ProductVariantId == productVariantId);

            if (basketItem != null)
                basketItem.Quantity += quantity;
            else
            {
                basket.Add(new BasketCookieItemViewModel
                {
                    ProductVariantId = productVariantId,
                    Quantity = quantity
                });
            }

            SaveBasketToCookie(basket);
        }

        // NEW: Async add that returns the updated BasketViewModel built from the in-memory list
        public async Task<BasketViewModel> AddToBasketAsync(int productVariantId, int quantity)
        {
            // Read current basket from request cookie (may be old), update the in-memory list and save.
            var basket = GetBasketFromCookie();
            var basketItem = basket.FirstOrDefault(item => item.ProductVariantId == productVariantId);

            if (basketItem != null)
                basketItem.Quantity += quantity;
            else
            {
                basket.Add(new BasketCookieItemViewModel
                {
                    ProductVariantId = productVariantId,
                    Quantity = quantity
                });
            }

            // Persist new state to response cookie
            SaveBasketToCookie(basket);

            // Build and return the BasketViewModel from the updated list (no reliance on Request.Cookies)
            return await BuildBasketViewModelAsync(basket);
        }

        public void RemoveFromBasket(int productVariantId)
        {
            var basket = GetBasketFromCookie();
            var basketItem = basket.FirstOrDefault(item => item.ProductVariantId == productVariantId);

            if (basketItem != null)
            {
                basket.Remove(basketItem);
                SaveBasketToCookie(basket);
            }
        }

        private List<BasketCookieItemViewModel> GetBasketFromCookie()
        {
            var cookieName = GetBasketCookieName();
            var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[cookieName];

            if (string.IsNullOrEmpty(cookie))
            {
                return new List<BasketCookieItemViewModel>();
            }

            try
            {
                var list = JsonSerializer.Deserialize<List<BasketCookieItemViewModel>>(cookie, _jsonOptions);
                return list ?? new List<BasketCookieItemViewModel>();
            }
            catch
            {
                return new List<BasketCookieItemViewModel>();
            }
        }

        private void SaveBasketToCookie(List<BasketCookieItemViewModel> basket)
        {
            var cookieName = GetBasketCookieName();
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30), // 30 gün yadda saxla
                HttpOnly = true
            };

            var cookieValue = JsonSerializer.Serialize(basket, _jsonOptions);

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(cookieName, cookieValue, cookieOptions);
        }

        public void CleanBasket()
        {
            var cookieName = GetBasketCookieName();
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(cookieName);
        }

        public void TransferGuestBasketToUser()
        {
            var guestCookieName = $"{BasketCookiePrefix}guest";
            var guestCookie = _httpContextAccessor.HttpContext?.Request.Cookies[guestCookieName];

            if (string.IsNullOrEmpty(guestCookie))
                return;

            List<BasketCookieItemViewModel> guestBasket;
            try
            {
                guestBasket = JsonSerializer.Deserialize<List<BasketCookieItemViewModel>>(guestCookie, _jsonOptions) ?? new List<BasketCookieItemViewModel>();
            }
            catch
            {
                guestBasket = new List<BasketCookieItemViewModel>();
            }

            var userBasket = GetBasketFromCookie();

            foreach (var guestItem in guestBasket)
            {
                var existingItem = userBasket.FirstOrDefault(x => x.ProductVariantId == guestItem.ProductVariantId);
                if (existingItem != null)
                {
                    existingItem.Quantity += guestItem.Quantity;
                }
                else
                {
                    userBasket.Add(guestItem);
                }
            }

            SaveBasketToCookie(userBasket);
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(guestCookieName);
        }

        // Helper: build a BasketViewModel from in-memory cookie list
        private async Task<BasketViewModel> BuildBasketViewModelAsync(List<BasketCookieItemViewModel> basket)
        {
            var basketViewModel = new BasketViewModel();

            foreach (var item in basket)
            {
                var productVariant = await _productVariantService.GetAsync(
                    predicate: x => x.Id == item.ProductVariantId,
                    include: x => x.Include(c => c.Color!));

                if (productVariant == null)
                    continue;

                var product = await _productService.GetByIdAsync(productVariant.ProductId);

                var displayPrice = productVariant.SalePrice ?? productVariant.Price;

                basketViewModel.Items.Add(new BasketItemViewModel
                {
                    ProductVariantId = productVariant.Id,
                    ProductName = product?.Name ?? string.Empty,
                    ImageName = productVariant?.CoverImageName ?? string.Empty,
                    Price = displayPrice,
                    Quantity = item.Quantity,
                    ColorName = productVariant?.ColorName ?? string.Empty,
                    Size = productVariant?.Size
                });
            }

            return basketViewModel;
        }
    }
}