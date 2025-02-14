using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using _234412H_AS2.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using static _234412H_AS2.Pages.Account.LoginWith2faModel;
using System.Security.Claims;
using _234412H_AS2.Attributes;
using Microsoft.AspNetCore.Identity;
using _234412H_AS2.Model;
using _234412H_AS2.Services;

namespace _234412H_AS2.Pages.Account
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IPasswordService _passwordService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly PasswordPolicyService _policyService;

        public ChangePasswordModel(
            IPasswordService passwordService,
            UserManager<ApplicationUser> userManager,
            PasswordPolicyService policyService)
        {
            _passwordService = passwordService;
            _userManager = userManager;
            _policyService = policyService;
        }

        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        {
            [Required]
            [DataType(DataType.Password)]
            [SanitizeInput]
            [Display(Name = "Current password")]
            public string CurrentPassword { get; set; }

            [Required]
            [SanitizeInput]
            [StringLength(100, MinimumLength = 12)]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        [TempData]
        public string StatusMessage { get; set; }

        [TempData]
        public string SuccessMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound();
            }

            if (!await _policyService.CanChangePassword(user))
            {
                ModelState.AddModelError(string.Empty,
                    "Password cannot be changed within 24 hours of the last change.");
                return Page();
            }

            var (success, message) = await _passwordService.ChangePasswordAsync(user.Id,
                Input.CurrentPassword, Input.NewPassword);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return Page();
            }

            await _policyService.UpdatePasswordDates(user);
            SuccessMessage = "Password changed successfully.";
            return RedirectToPage();
        }
    }
}
