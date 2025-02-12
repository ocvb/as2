using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using _234412H_AS2.Model;
using _234412H_AS2.Services.Interfaces;
using QRCoder;

namespace _234412H_AS2.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthContextDb _context;

        public AuthenticationService(UserManager<ApplicationUser> userManager, AuthContextDb context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> Enable2faAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.SetTwoFactorEnabledAsync(user, true);
            if (result.Succeeded)
            {
                var twoFactorAuth = new TwoFactorAuth
                {
                    UserId = userId,
                    IsEnabled = true,
                    EnabledDate = DateTime.UtcNow,
                    SecretKey = await _userManager.GetAuthenticatorKeyAsync(user)
                };

                _context.TwoFactorAuths.Add(twoFactorAuth);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<(byte[] qrCodeImage, string manualEntryKey)> Generate2faSetupInfoAsync(string userId, string email)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (null, null);

            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

                // Store the key in TwoFactorAuth table
                var existingEntry = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
                if (existingEntry != null)
                {
                    existingEntry.SecretKey = unformattedKey;
                }
                else
                {
                    _context.TwoFactorAuths.Add(new TwoFactorAuth
                    {
                        UserId = userId,
                        SecretKey = unformattedKey,
                        IsEnabled = false,
                        EnabledDate = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }

            var authenticatorUri = $"otpauth://totp/{Uri.EscapeDataString(email)}?secret={unformattedKey}&issuer=FreshFarmMarket";

            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(authenticatorUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            return (qrCodeBytes, unformattedKey);
        }

        public async Task<bool> Disable2faAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var result = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (result.Succeeded)
            {
                var twoFactorAuth = await _context.TwoFactorAuths.FirstOrDefaultAsync(t => t.UserId == userId);
                if (twoFactorAuth != null)
                {
                    twoFactorAuth.IsEnabled = false;
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            return false;
        }

        public async Task<string> Generate2faTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return null;

            return await _userManager.GenerateTwoFactorTokenAsync(user, "Authenticator");
        }

        public async Task<bool> Verify2faTokenAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            return await _userManager.VerifyTwoFactorTokenAsync(user, "Authenticator", token);
        }

        public async Task<bool> GetTwoFactorEnabledAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            var twoFactorAuth = await _context.TwoFactorAuths
                .FirstOrDefaultAsync(t => t.UserId == userId);

            return twoFactorAuth?.IsEnabled ?? false;
        }
    }
}
