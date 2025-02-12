using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authorization;
using _234412H_AS2.Services.Interfaces;

namespace _234412H_AS2.Pages.Account.Manage
{
    [Authorize]
    public class TwoFactorAuthenticationModel : PageModel
    {
        private readonly IAuthenticationService _authService;

        public TwoFactorAuthenticationModel(IAuthenticationService authService)
        {
            _authService = authService;
        }

        public bool Is2faEnabled { get; set; }
        public byte[] QrCodeImage { get; set; }
        [TempData]
        public string ErrorMessage { get; set; }

        public string ManualEntryKey { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            Is2faEnabled = await _authService.GetTwoFactorEnabledAsync(userId);
            if (!Is2faEnabled)
            {
                var (qrCode, manualKey) = await _authService.Generate2faSetupInfoAsync(userId, userEmail);
                QrCodeImage = qrCode;
                ManualEntryKey = manualKey;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostVerifyAndEnableAsync(string verificationCode)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(verificationCode) || verificationCode.Length != 6)
            {
                ErrorMessage = "Please enter a valid 6-digit code.";
                Is2faEnabled = false;
                var (qrCode, manualKey) = await _authService.Generate2faSetupInfoAsync(userId, userEmail);
                QrCodeImage = qrCode;
                ManualEntryKey = manualKey;
                return Page();
            }

            var isValid = await _authService.Verify2faTokenAsync(userId, verificationCode);
            if (isValid)
            {
                await _authService.Enable2faAsync(userId);
                return RedirectToPage();
            }

            ErrorMessage = "Invalid verification code. Please try again.";
            Is2faEnabled = false;
            var (newQrCode, newManualKey) = await _authService.Generate2faSetupInfoAsync(userId, userEmail);
            QrCodeImage = newQrCode;
            ManualEntryKey = newManualKey;
            return Page();
        }

        public async Task<IActionResult> OnPostDisableAsync()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _authService.Disable2faAsync(userId);
            return RedirectToPage();
        }
    }
}
