using EcommerceCoza.BLL.Services.Contracts;
using EcommerceCoza.BLL.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EcommerceCoza.BLL.Services
{
    public class BasketManager
    {
        private const string BasketCookieName = "basketProject";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IProductService _productService;
        private readonly IProductVariantService _productVariantService;

        public BasketManager(IProductService productService, IHttpContextAccessor httpContextAccessor, IProductVariantService productVariantService)
        {
            _productService = productService;
            _httpContextAccessor = httpContextAccessor;
            _productVariantService = productVariantService;
        }

        public async Task<BasketViewModel> GetBasketAsync()
        {
            var basket = GetBasketFromCookie();
            var basketViewModel = new BasketViewModel();

            foreach(var item in basket)
            {
                var productVariant = await _productVariantService.GetAsync(predicate:x=>x.Id==item.ProductVariantId,
                    include: x=>x.Include(c=>c.Color!));
                
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
                        ColorName=productVariant?.ColorName!
                    });
                }
            }

            return basketViewModel;
        }

        public async Task<BasketViewModel> ChangeQuantityAsync(int productVariantId, int quantity)
        {
            var basket = GetBasketFromCookie();
            var basketItem = basket.FirstOrDefault(item=>item.ProductVariantId==productVariantId);

            if (basketItem != null)
            {
                basketItem.Quantity += quantity;

                SaveBasketToCookie(basket);
            }

            var basketViewModel = new BasketViewModel();

            foreach(var item in basket)
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
                            Quantity=item.Quantity,
                            ColorName=productVariant.ColorName!,
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
                basketItem.Quantity+=quantity;
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
            var basketItem = basket.FirstOrDefault(item=>item.ProductVariantId==productVariantId);

            if (basketItem != null)
            {
                basket.Remove(basketItem);
                SaveBasketToCookie(basket);
            }
        }

        private List<BasketCookieItemViewModel> GetBasketFromCookie()
        {
            var cookie = _httpContextAccessor.HttpContext?.Request.Cookies[BasketCookieName];

            if(string.IsNullOrEmpty(cookie))
            {
                return new List<BasketCookieItemViewModel>();
            }

            return JsonSerializer.Deserialize<List<BasketCookieItemViewModel>>(cookie) ?? [];
        }

        private void SaveBasketToCookie(List<BasketCookieItemViewModel> basket)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                HttpOnly=true
            };

            var cookieValue = JsonSerializer.Serialize(basket);

            _httpContextAccessor.HttpContext?.Response.Cookies.Append(BasketCookieName, cookieValue, cookieOptions);
        }

        public void CleanBasket()
        {
            var basket = GetBasketFromCookie();

            basket.Clear();

            SaveBasketToCookie(basket);
        }
    }
}
