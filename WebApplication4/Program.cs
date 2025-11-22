using ECommerceCoza.BLL.Constants;
using ECommerceCoza.DAL.DataContext.Entities;
using ECommerceCoza.DAL.DataContext;
using Microsoft.AspNetCore.Identity;
using EcommerceCoza.BLL;
using EcommerceCoza.DAL;
using ECommerceCoza.BLL.Services;
using ECommerceCoza.BLL.Services.Contracts;
using Stripe;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using WebApplication4.Services;
using WebApplication4.Models;

namespace EcommerceCoza.MVC
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrencyService, CurrencyService>();

            // Localization
            builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

            builder.Services
                .AddControllersWithViews()
                .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            builder.Services.AddDataAccessLayerServices(builder.Configuration);
            builder.Services.AddBusinessLogicLayerServices();
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

            builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 3;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

            // Configure Identity options for Access Denied
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.LoginPath = "/Account/Login";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
            });

            FilePathConstants.ProductImagePath = Path.Combine(builder.Environment.WebRootPath, "images", "products");
            FilePathConstants.CategoryImagePath = Path.Combine(builder.Environment.WebRootPath, "images", "collections");

            var app = builder.Build();

            app.Use(async (ctx, next) =>
            {
                // Ensure currency cookie exists
                if (!ctx.Request.Cookies.ContainsKey("CurrentCurrency"))
                {
                    ctx.Response.Cookies.Append("CurrentCurrency", Currency.USD.ToString(), new CookieOptions
                    {
                        HttpOnly = false,
                        IsEssential = true,
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    });
                }
                await next();
            });

            // Supported cultures
            var supportedCultures = new[] { "en", "az", "ru" };
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture("en")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures)
                .AddInitialRequestCultureProvider(new CookieRequestCultureProvider());

            app.UseRequestLocalization(localizationOptions);

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            using (var scope = app.Services.CreateScope())
            {
                var dataInitializer = scope.ServiceProvider.GetRequiredService<DataInitializer>();
                await dataInitializer.Initialize();
            }

            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                string adminRole = "Admin";
                string adminEmail = "admin@gmail.com";
                string adminPassword = "admin123";

                if (!await roleManager.RoleExistsAsync(adminRole))
                    await roleManager.CreateAsync(new IdentityRole(adminRole));

                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    adminUser = new AppUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FirstName = "Admin",
                        LastName = "User"
                    };
                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Admin creation error: {error.Description}");
                        }
                    }
                }

                // Assign Admin Role
                if (!await userManager.IsInRoleAsync(adminUser, adminRole))
                {
                    var roleResult = await userManager.AddToRoleAsync(adminUser, adminRole);
                    if (roleResult.Succeeded)
                    {
                        System.Diagnostics.Debug.WriteLine($"Admin role assigned to {adminEmail}");
                    }
                }
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}