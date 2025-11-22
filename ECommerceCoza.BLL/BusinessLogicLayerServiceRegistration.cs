using EcommerceCoza.BLL.Services;
using EcommerceCoza.BLL.Services.Contracts;
using ECommerceCoza.BLL.Mapping;
using ECommerceCoza.BLL.Services;
using ECommerceCoza.BLL.Services.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceCoza.BLL
{
    public static class BusinessLogicLayerServiceRegistration
    {
        public static IServiceCollection AddBusinessLogicLayerServices(this IServiceCollection services)
        {
            services.AddAutoMapper(config => config.AddProfile<MappingProfile>());
            services.AddScoped(typeof(ICrudService<,,,>), typeof(CrudManager<,,,>));

            services.AddScoped<ICategoryService, CategoryManager>();
            services.AddScoped<IProductService, ProductManager>();
            services.AddScoped<IProductVariantService, ProductVariantManager>();
            services.AddScoped<IBioService, BioManager>();
            services.AddScoped<ISocialService, SocialManager>();
            services.AddScoped<IAddressService, AddressManager>();
            services.AddScoped<IWishlistItemService, WishlistItemManager>();
            services.AddScoped<IOrderService, OrderManager>();
            services.AddScoped<IOrderDetailService, OrderDetailManager>();
            services.AddScoped<IDiscountCodeService, DiscountCodeManager>();
            services.AddScoped<IColorService, ColorManager>();
            services.AddScoped<IBrandService, BrandManager>();
            services.AddScoped<IHomeService, HomeManager>();
            services.AddScoped<IShopService, ShopManager>();
            services.AddScoped<IHeaderService, HeaderManager>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFooterService, FooterManager>();

            services.AddScoped<BasketManager>();
            services.AddScoped<FileService>();

            return services;
        }
    }
}
