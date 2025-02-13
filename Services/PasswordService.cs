using Microsoft.AspNetCore.Identity;
using _234412H_AS2.Model;
using _234412H_AS2.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace _234412H_AS2.Services
{
    public class PasswordService(UserManager<ApplicationUser> userManager, AuthContextDb context) : IPasswordService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly AuthContextDb _context = context;
        private const int MAX_PASSWORD_HISTORY = 2;

        public async Task<(bool success, string message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return (false, "User not found.");

            // Check password history
            var passwordHistories = await _context.PasswordHistories
                .Where(ph => ph.UserId == userId)
                .OrderByDescending(ph => ph.CreatedAt)
                .Take(MAX_PASSWORD_HISTORY)
                .ToListAsync();

            foreach (var history in passwordHistories)
            {
                var verificationResult = _userManager.PasswordHasher.VerifyHashedPassword(
                    user, history.HashedPassword, newPassword);

                if (verificationResult == PasswordVerificationResult.Success)
                {
                    return (false, "Cannot reuse any of your last 2 passwords. Please choose a different password.");
                }
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                // Add new password to history
                var newHashedPassword = _userManager.PasswordHasher.HashPassword(user, newPassword);
                _context.PasswordHistories.Add(new PasswordHistory
                {
                    UserId = userId,
                    HashedPassword = newHashedPassword,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();

                // Clean up old history entries
                var oldEntries = await _context.PasswordHistories
                    .Where(ph => ph.UserId == userId)
                    .OrderByDescending(ph => ph.CreatedAt)
                    .Skip(MAX_PASSWORD_HISTORY)
                    .ToListAsync();

                if (oldEntries.Any())
                {
                    _context.PasswordHistories.RemoveRange(oldEntries);
                    await _context.SaveChangesAsync();
                }

                return (true, "Password changed successfully.");
            }

            return (false, "Failed to change password. Please ensure your current password is correct.");
        }
    }
}
