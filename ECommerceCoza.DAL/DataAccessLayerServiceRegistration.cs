using EcommerceCoza.DAL.DataContext.Repositories;
using EcommerceCoza.DAL.DataContext.Repositories.Contracts;
using ECommerceCoza.DAL.DataContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EcommerceCoza.DAL
{
    public static class DataAccessLayerServiceRegistration
    {
        public static IServiceCollection AddDataAccessLayerServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
           options.UseSqlServer(configuration.GetConnectionString("Default"), options =>
           {
               options.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
           }));

            services.AddScoped<DataInitializer>();

            services.AddScoped(typeof(IRepository<>), typeof(EFCoreRepository<>));

            services.AddScoped<IBrandRepository, BrandRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>(); 
            services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
            services.AddScoped<IBioRepository, BioRepository>();
            services.AddScoped<ISocialRepository, SocialRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            services.AddScoped<IWishlistItemRepository, WishlistItemRepository>();
            services.AddScoped<IDiscountCodeRepository, DiscountCodeRepository>();
            services.AddScoped<IAddressRepository, AddressRepository>();
            services.AddScoped<IColorRepository, ColorRepository>();

            return services;
        }
    }
}
