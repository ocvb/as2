using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _234412H_AS2.Model;
using _234412H_AS2.Services;
using _234412H_AS2.Middleware;
using Microsoft.AspNetCore.DataProtection;
using _234412H_AS2.Validators;
using _234412H_AS2.Services.Interfaces;

namespace _234412H_AS2;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder.Services, builder.Configuration, builder.Environment);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        var app = builder.Build();

        InitializeDatabase(app);
        ConfigureMiddleware(app, app.Environment);

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
    {
        // Basic Services
        services.AddRazorPages();
        services.AddHttpContextAccessor();
        services.AddDistributedMemoryCache();

        // Database
        services.AddDbContext<AuthContextDb>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));

        // Security Services
        ConfigureIdentity(services);
        ConfigureAuthentication(services);
        ConfigureSession(services);
        ConfigureRecaptcha(services, configuration);
        ConfigureDataProtection(services, env);

        // Application Services
        RegisterApplicationServices(services);
    }

    private static void ConfigureIdentity(IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 12;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 3;
            options.Lockout.AllowedForNewUsers = true;
        })
        .AddEntityFrameworkStores<AuthContextDb>()
        .AddDefaultTokenProviders()
        .AddPasswordValidator<CustomPasswordValidator<ApplicationUser>>();

        services.Configure<SecurityStampValidatorOptions>(options =>
            options.ValidationInterval = TimeSpan.FromMinutes(5));
    }

    private static void ConfigureAuthentication(IServiceCollection services)
    {
        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
            options.AccessDeniedPath = "/AccessDenied";
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.SameSite = SameSiteMode.Strict;
        });
    }

    private static void ConfigureSession(IServiceCollection services)
    {
        services.AddSession(options =>
        {
            options.Cookie = new CookieBuilder
            {
                Name = ".FFM.Session",
                HttpOnly = true,
                IsEssential = true,
                SecurePolicy = CookieSecurePolicy.Always,
                SameSite = SameSiteMode.Strict
            };
            options.IdleTimeout = TimeSpan.FromMinutes(3);
        });
    }

    private static void ConfigureRecaptcha(IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("RecaptchaClient", client =>
            client.BaseAddress = new Uri("https://www.google.com/recaptcha/api/"));

        var recaptchaConfig = configuration.GetSection("Recaptcha");
        if (string.IsNullOrEmpty(recaptchaConfig["SiteKey"]) || string.IsNullOrEmpty(recaptchaConfig["SecretKey"]))
        {
            throw new InvalidOperationException("reCAPTCHA configuration is missing or invalid");
        }

        services.AddScoped<IRecaptchaService, RecaptchaEnterpriseService>();
    }

    private static void ConfigureDataProtection(IServiceCollection services, IWebHostEnvironment env)
    {
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(env.ContentRootPath, "Keys")))
            .SetApplicationName("234412H_AS2")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(14));
    }

    private static void RegisterApplicationServices(IServiceCollection services)
    {
        services.AddScoped<ILockoutService, LockoutService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddHostedService<BackgroundBackupService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddScoped<AuditService>();
        services.AddScoped<PasswordPolicyService>();
    }

    private static void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
    {
        // Error handling
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Error/500");
            app.UseStatusCodePagesWithReExecute("/Error/{0}");
        }
        else
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseSession();
        app.UseMiddleware<SecurityHeadersMiddleware>();
        app.UseMiddleware<SessionMiddleware>();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapRazorPages();
    }

    private static void InitializeDatabase(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        try
        {
            var context = services.GetRequiredService<AuthContextDb>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            context.Database.EnsureCreated();

            if (!roleManager.RoleExistsAsync("User").Result)
            {
                var role = new IdentityRole("User");
                roleManager.CreateAsync(role).Wait();
            }
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while initializing the database.");
        }
    }
}
