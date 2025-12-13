using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace EcommerceCoza.BLL.Services
{
    public class BasketManager
    {
        private const string BasketCookiePrefix = "basket_";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly IProductVariantService _productVariantService;

        public BasketManager(IProductService productService, IHttpContextAccessor httpContextAccessor, IProductVariantService productVariantService)
        {
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
            _productVariantService = productVariantService;
        }

        private string GetBasketCookieName()
        {
            // Əgər istifadəçi giriş edibsə, onun ID-si ilə cookie adı
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                return $"{BasketCookiePrefix}{userId}";
            }

            // Əgər giriş etməyibsə, ümumi cookie
            return $"{BasketCookiePrefix}guest";
        }

        public async Task<BasketViewModel> GetBasketAsync()
        {
            var basket = GetBasketFromCookie();
            var basketViewModel = new BasketViewModel();

            foreach (var item in basket)
            {
                var productVariant = await _productVariantService.GetAsync(predicate: x => x.Id == item.ProductVariantId,
                    include: x => x.Include(c => c.Color!));

                if (productVariant != null)
                {
                    var product = await _productService.GetByIdAsync(productVariant.ProductId);
                    basketViewModel.Items.Add(new BasketItemViewModel
                    {
                        ProductVariantId = productVariant.Id,
                        ProductName = product?.Name!,
                        ImageName = productVariant?.CoverImageName!,
                        Price = product!.BasePrice,
                        Quantity = item.Quantity,
                        ColorName = productVariant?.ColorName!
                    });
                }
            }

            return basketViewModel;
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

            var basketViewModel = new BasketViewModel();

            foreach (var item in basket)
            {
                var productVariant = await _productVariantService.GetAsync(predicate: x => x.Id == item.ProductVariantId,
                     include: x => x.Include(c => c.Color!));

                if (productVariant != null)
                {
                    var product = await _productService.GetByIdAsync(productVariant.ProductId);

                    if (product != null)
                    {
                        basketViewModel.Items.Add(new BasketItemViewModel
                        {
                            ProductVariantId = productVariant.Id,
                            ProductName = product.Name!,
                            ImageName = productVariant.CoverImageName!,
                            Price = product.BasePrice,
                            Quantity = item.Quantity,
                            ColorName = productVariant.ColorName!,
                        });
                    }
                }
            }

            return basketViewModel;
        }

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

            return JsonSerializer.Deserialize<List<BasketCookieItemViewModel>>(cookie) ?? [];
        }

        private void SaveBasketToCookie(List<BasketCookieItemViewModel> basket)
        {
            var cookieName = GetBasketCookieName();
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(30), // 30 gün yadda saxla
                HttpOnly = true
            };

            var cookieValue = JsonSerializer.Serialize(basket);

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(cookieName, cookieValue, cookieOptions);
        }

        public void CleanBasket()
        {
            var cookieName = GetBasketCookieName();
            _httpContextAccessor.HttpContext?.Response.Cookies.Delete(cookieName);
        }

        public void TransferGuestBasketToUser()
        {
            // Guest cookie-ni oxu
            var guestCookieName = $"{BasketCookiePrefix}guest";
            var guestCookie = _httpContextAccessor.HttpContext?.Request.Cookies[guestCookieName];

            if (!string.IsNullOrEmpty(guestCookie))
            {
                // Guest səbətini user cookie-nə köçür
                var guestBasket = JsonSerializer.Deserialize<List<BasketCookieItemViewModel>>(guestCookie) ?? [];
                var userBasket = GetBasketFromCookie();

                // Merge et
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

                // Guest cookie-ni sil
                _httpContextAccessor.HttpContext?.Response.Cookies.Delete(guestCookieName);
            }
        }
    }
}