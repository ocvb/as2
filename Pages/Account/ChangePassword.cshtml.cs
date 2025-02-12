using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using _234412H_AS2.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

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
            [Display(Name = "Current password")]
            public string CurrentPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "New password")]
            public string NewPassword { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm new password")]
            [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var result = await _passwordService.ChangePasswordAsync(userId, Input.CurrentPassword, Input.NewPassword);

            if (result)
            {
                TempData["StatusMessage"] = "Your password has been changed.";
                return RedirectToPage("/Index");
            }

            ModelState.AddModelError(string.Empty, "Error changing password. Please verify your current password.");
            return Page();
        }
    }
}
