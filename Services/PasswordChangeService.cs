using Microsoft.AspNetCore.Identity;
using _234412H_AS2.Model;
using _234412H_AS2.Services.Interfaces;

namespace _234412H_AS2.Services
{
    public class PasswordChangeService(UserManager<ApplicationUser> userManager) : IPasswordChangeService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            return result.Succeeded;
        }

        public async Task<bool> ValidatePasswordAsync(string userId, string password)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return false;

            return await _userManager.CheckPasswordAsync(user, password);
        }
    }
}
