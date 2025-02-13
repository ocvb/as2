using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using _234412H_AS2.Model;
using _234412H_AS2.Services.Interfaces;
using System.Text.Encodings.Web;

namespace _234412H_AS2.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailService emailService,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                StatusMessage = "If your email is registered, you will receive a password reset link shortly.";
                return RedirectToPage();
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { code },
                protocol: Request.Scheme);

            var emailBody = $@"
            <html>
            <body style='font-family: Arial, sans-serif; padding: 20px;'>
                <h2>Reset Your Password</h2>
                <p>You have requested to reset your password. Please click the button below to proceed:</p>
                <p style='margin: 25px 0;'>
                    <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'
                       style='background-color: #007bff; color: white; padding: 10px 20px;
                              text-decoration: none; border-radius: 5px; display: inline-block;'>
                        Reset Password
                    </a>
                </p>
                <p>If you did not request this password reset, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
                <hr style='margin: 20px 0;'>
                <p style='font-size: 12px; color: #666;'>
                    For security reasons, please do not forward this email to anyone.
                </p>
            </body>
            </html>";

            await _emailService.SendEmailAsync(
                Input.Email,
                "Reset Your Password - Fresh Farm Market",
                emailBody);

            StatusMessage = "If your email is registered, you will receive a password reset link shortly.";
            return RedirectToPage();
        }
    }
}
