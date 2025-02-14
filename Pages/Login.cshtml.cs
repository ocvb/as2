using System.ComponentModel.DataAnnotations;
using _234412H_AS2.Model;
using _234412H_AS2.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _234412H_AS2.Model;
using _234412H_AS2.Services;
using System.Security.Claims;

namespace _234412H_AS2.Pages
{
    public class LoginModel(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        IRecaptchaService recaptchaService,
        IConfiguration configuration,
        ISessionService sessionService,
        IEncryptionService encryptionService,
        AuditService auditService,
        ILogger<LoginModel> logger) : PageModel  // Changed to typed logger
    {
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IRecaptchaService _recaptchaService = recaptchaService;
        private readonly IConfiguration _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        private readonly ISessionService _sessionService = sessionService;
        private readonly IEncryptionService _encryptionService = encryptionService;
        private readonly AuditService _auditService = auditService;
        private readonly ILogger<LoginModel> _logger = logger;  // Changed to typed logger

        [BindProperty]
        public required LoginInputModel Input { get; set; }

        public string RecaptchaSiteKey => _configuration["Recaptcha:SiteKey"] ??
            throw new InvalidOperationException("Recaptcha:SiteKey not found in configuration");

        [BindProperty(SupportsGet = false)]
        public required string RecaptchaToken { get; set; }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            if (string.IsNullOrEmpty(RecaptchaToken))
            {
                ModelState.AddModelError(string.Empty, "reCAPTCHA verification required.");
                return Page();
            }

            try
            {
                if (!await _recaptchaService.VerifyToken(RecaptchaToken))
                {
                    ModelState.AddModelError(string.Empty, "reCAPTCHA verification failed. Please try again.");
                    return Page();
                }

                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: true);

                    if (result.RequiresTwoFactor)
                    {
                        return RedirectToPage("./Account/LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
                    }

                    // In the OnPostAsync method, after successful login:
                    if (result.Succeeded)
                    {
                        var user = await _userManager.FindByEmailAsync(Input.Email);
                        var policyService = new PasswordPolicyService(_userManager);

                        if (await policyService.IsPasswordExpired(user))
                        {
                            return RedirectToPage("/ChangePassword", new { message = "Your password has expired. Please change it." });
                        }

                        await _auditService.LogAction(
                            User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Input.Email,
                            "Login",
                            "User logged in successfully"
                        );

                        if (user == null)
                        {

                            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                            return Page();
                        }

                        var sessionId = _sessionService.GenerateSessionId();
                        _sessionService.SetSessionId(sessionId);
                        _sessionService.SetUserData("EncryptedEmail", _encryptionService.EncryptData(user.Email));

                        return RedirectToPage("/Index");
                    }
                    else
                    {
                        await _auditService.LogAction(
                            Input.Email,
                            "Login Failed",
                            $"Failed login attempt for user {Input.Email}"
                        );
                    }

                    if (result.IsLockedOut)
                    {
                        var user = await _userManager.FindByEmailAsync(Input.Email);
                        if (user != null)
                        {
                            var lockoutEnd = DateTimeOffset.Now.AddMinutes(5);
                            _logger.LogWarning("User account {Email} locked out until {LockoutEnd}",
                                user.Email,
                                lockoutEnd.ToString("O"));
                            await _userManager.SetLockoutEndDateAsync(user, lockoutEnd);
                        }
                        return RedirectToPage("./Lockout");
                    }

                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }

            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                // Log the exception here if you have a logging service
                await _auditService.LogAction(
                    Input.Email ?? "Unknown",
                    "Login Error",
                    $"Error during login: {ex.Message}"
                );
                throw;
            }

            return Page();
        }
    }

    public class LoginInputModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
