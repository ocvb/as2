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
        private readonly ILogger<TwoFactorAuthenticationModel> _logger;

        public TwoFactorAuthenticationModel(
            IAuthenticationService authService,
            ILogger<TwoFactorAuthenticationModel> logger)
        {
            _authService = authService;
            _logger = logger;
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

        [BindProperty]
        public string VerificationCode { get; set; }

        public async Task<IActionResult> OnPostVerifyAndEnableAsync()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

            _logger.LogInformation("Attempting to verify 2FA code for user {UserId}", userId);

            try
            {
                if (string.IsNullOrEmpty(VerificationCode) || VerificationCode.Length != 6)
                {
                    _logger.LogWarning("Invalid verification code format for user {UserId}", userId);
                    ErrorMessage = "Please enter a valid 6-digit code.";
                    Is2faEnabled = false;
                    var (qrCode, manualKey) = await _authService.Generate2faSetupInfoAsync(userId, userEmail);
                    QrCodeImage = qrCode;
                    ManualEntryKey = manualKey;
                    return Page();
                }

                _logger.LogInformation("Verifying code: {Code} for user {UserId}", VerificationCode, userId);

                // First disable any existing 2FA
                await _authService.Disable2faAsync(userId);

                var isValid = await _authService.Verify2faTokenAsync(userId, VerificationCode);

                if (isValid)
                {
                    _logger.LogInformation("2FA verification successful for user {UserId}", userId);
                    await _authService.Enable2faAsync(userId);
                    Is2faEnabled = true;
                    return RedirectToPage();
                }

                _logger.LogWarning("Invalid verification code attempt for user {UserId}", userId);
                ErrorMessage = "Invalid verification code. Please try again.";
                Is2faEnabled = false;
                var (newQrCode, newManualKey) = await _authService.Generate2faSetupInfoAsync(userId, userEmail);
                QrCodeImage = newQrCode;
                ManualEntryKey = newManualKey;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enabling 2FA for user {UserId}", userId);
                ErrorMessage = "An error occurred while enabling 2FA. Please try again.";
                Is2faEnabled = false;
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDisableAsync()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            await _authService.Disable2faAsync(userId);
            return RedirectToPage();
        }
    }
}
