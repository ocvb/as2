using Microsoft.AspNetCore.Identity;
using _234412H_AS2.Model;

namespace _234412H_AS2.Services
{
    public interface ILockoutService
    {
        Task<DateTimeOffset?> GetLockoutEndAsync(ApplicationUser user);
        Task SetLockoutAsync(ApplicationUser user);
    }

    public class LockoutService : ILockoutService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public LockoutService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<DateTimeOffset?> GetLockoutEndAsync(ApplicationUser user)
        {
            return await _userManager.GetLockoutEndDateAsync(user);
        }

        public async Task SetLockoutAsync(ApplicationUser user)
        {
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(5));
            await _userManager.UpdateSecurityStampAsync(user);
        }
    }
}
