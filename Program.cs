using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _234412H_AS2.Model;
using _234412H_AS2.Services;
using _234412H_AS2.Middleware;
using Microsoft.AspNetCore.DataProtection;

namespace _234412H_AS2;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorPages();
        builder.Services.AddHttpContextAccessor();

        // Configure HttpClient for reCAPTCHA
        builder.Services.AddHttpClient("RecaptchaClient", client =>
        {
            client.BaseAddress = new Uri("https://www.google.com/recaptcha/api/");
        });

        // Verify reCAPTCHA configuration
        var recaptchaConfig = builder.Configuration.GetSection("Recaptcha");
        if (string.IsNullOrEmpty(recaptchaConfig["SiteKey"]) || string.IsNullOrEmpty(recaptchaConfig["SecretKey"]))
        {
            throw new InvalidOperationException("reCAPTCHA configuration is missing or invalid");
        }

        // Replace the existing reCAPTCHA service registration
        builder.Services.AddScoped<IRecaptchaService, RecaptchaEnterpriseService>();

        builder.Services.AddDbContext<AuthContextDb>(options =>
        {
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
        });

        // Add Data Protection
        builder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "Keys")))
            .SetApplicationName("234412H_AS2")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(14));

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 12;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 3;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<AuthContextDb>()
        .AddDefaultTokenProviders();

        builder.Services.AddScoped<IEncryptionService, EncryptionService>();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
            options.AccessDeniedPath = "/AccessDenied";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });

        // Simplified Session Configuration
        builder.Services.AddDistributedMemoryCache();
        builder.Services.AddSession(options =>
        {
            options.Cookie = new CookieBuilder
            {
                Name = ".FFM.Session",
                HttpOnly = true,
                IsEssential = true,
                SecurePolicy = CookieSecurePolicy.Always,
                SameSite = SameSiteMode.Strict
            };
            options.IdleTimeout = TimeSpan.FromMinutes(5);
        });

        builder.Services.AddSingleton<ISessionService, SessionService>();

        builder.Services.AddScoped<AuditService>();

        var app = builder.Build();

        // Configure error handling
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
            app.UseHsts();
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        // Add logging middleware for debugging
        app.Use(async (context, next) =>
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
            logger.LogInformation($"Request Path: {context.Request.Path}");
            await next();
        });

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Correct middleware order
        app.UseSession();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<SessionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }


}
