using _234412H_AS2.Attributes;
using _234412H_AS2.Model;
using _234412H_AS2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _234412H_AS2.Pages;

[ValidateSession]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEncryptionService _encryptionService;
    private readonly IRecaptchaService _recaptchaService;
    private readonly IConfiguration _configuration;

    public IndexModel(
        ILogger<IndexModel> logger,
        UserManager<ApplicationUser> userManager,
        IEncryptionService encryptionService,
        IRecaptchaService recaptchaService,
        IConfiguration configuration)
    {
        _logger = logger;
        _userManager = userManager;
        _encryptionService = encryptionService;
        _recaptchaService = recaptchaService;
        _configuration = configuration;
    }


    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public string? UserPhone { get; set; }
    public string? UserAddress { get; set; }
    public string? UserGender { get; set; }
    public string? UserAboutMe { get; set; }
    public string? UserProfilePicture { get; set; }


    public async Task<IActionResult> OnGetAsync()
    {

        var user = await _userManager.GetUserAsync(User);
        if (user != null)
        {
            UserFullName = _encryptionService.DecryptData(user.FullName);
            UserEmail = user.Email;
            UserPhone = _encryptionService.DecryptData(user.PhoneNumber);
            UserAddress = _encryptionService.DecryptData(user.Address);
            UserGender = _encryptionService.DecryptData(user.Gender);

            if (!string.IsNullOrEmpty(user.AboutMe))
            {
                UserAboutMe = _encryptionService.DecryptData(user.AboutMe);
            }

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                UserProfilePicture = $"/uploads/{user.ProfilePicture}";
            }
        }

        return Page();
    }
}
