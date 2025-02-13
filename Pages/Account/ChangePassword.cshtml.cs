using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using _234412H_AS2.Services.Interfaces;
using System.ComponentModel.DataAnnotations;
using static _234412H_AS2.Pages.Account.LoginWith2faModel;
using System.Security.Claims;
using _234412H_AS2.Attributes;

namespace _234412H_AS2.Pages.Account
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IPasswordService _passwordService;

        public ChangePasswordModel(IPasswordService passwordService)
        {
            _passwordService = passwordService;
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

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var (success, message) = await _passwordService.ChangePasswordAsync(userId,
                Input.CurrentPassword, Input.NewPassword);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, message);
                return Page();
            }

            SuccessMessage = "Password changed successfully.";
            return RedirectToPage();
        }
    }
}
