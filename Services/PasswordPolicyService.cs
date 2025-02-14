using Microsoft.AspNetCore.Identity;
using _234412H_AS2.Model;

namespace _234412H_AS2.Services
{
    public class PasswordPolicyService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private const int MinPasswordAge = 1;
        private const int MaxPasswordAge = 30;

        public PasswordPolicyService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> CanChangePassword(ApplicationUser user)
        {
            if (user.LastPasswordChangeDate == null)
                return true;

            var daysSinceLastChange = (DateTime.Now - user.LastPasswordChangeDate.Value).TotalSeconds;
            return daysSinceLastChange >= MinPasswordAge;
        }

        public async Task UpdatePasswordDates(ApplicationUser user)
        {
            user.LastPasswordChangeDate = DateTime.Now;
            user.PasswordExpiryDate = DateTime.Now.AddSeconds(MaxPasswordAge);
            await _userManager.UpdateAsync(user);
        }

        public async Task<bool> IsPasswordExpired(ApplicationUser user)
        {
            return user.PasswordExpiryDate.HasValue &&
                   DateTime.Now > user.PasswordExpiryDate.Value;
        }
    }
}
