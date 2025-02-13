using _234412H_AS2.ViewModels;
using Microsoft.AspNetCore.Identity;
using _234412H_AS2.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _234412H_AS2.Services;

namespace _234412H_AS2.Pages
{
    public class RegisterModel(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IWebHostEnvironment environment,
        IEncryptionService encryptionService
            ) : PageModel
    {
        private UserManager<ApplicationUser> _userManager { get; } = userManager;
        private SignInManager<ApplicationUser> _signInManager { get; } = signInManager;
        private IWebHostEnvironment _environment { get; } = environment;
        private readonly IEncryptionService _encryptionService = encryptionService;


        [BindProperty]
        public required Register RModel { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _userManager.FindByEmailAsync(RModel.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "Email address is already registered.");
                    return Page();
                }

                var user = new ApplicationUser()
                {
                    UserName = RModel.Email,
                    Email = RModel.Email,
                    FullName = _encryptionService.EncryptData(RModel.FullName),
                    CreditCardNumber = _encryptionService.EncryptData(RModel.CreditCardNumber),
                    Gender = _encryptionService.EncryptData(RModel.Gender),
                    PhoneNumber = _encryptionService.EncryptData(RModel.Phone),
                    Address = _encryptionService.EncryptData(RModel.Address),
                    AboutMe = _encryptionService.EncryptData(RModel.AboutMe ?? ""),
                };

                if (Request.Form.Files.Count > 0)
                {
                    var file = Request.Form.Files[0];
                    if (file != null)
                    {
                        var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                        var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        if (!Directory.Exists(uploadsFolder))
                            Directory.CreateDirectory(uploadsFolder);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        user.ProfilePicture = uniqueFileName;
                    }
                }

                var result = await _userManager.CreateAsync(user, RModel.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToPage("/Index");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return Page();
        }

        public void OnGet()
        {
        }
    }
}
