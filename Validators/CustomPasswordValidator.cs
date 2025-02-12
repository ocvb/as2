using Microsoft.AspNetCore.Identity;

namespace _234412H_AS2.Validators
{
    public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            // Check if user is locked out
            if (await manager.IsLockedOutAsync(user))
            {
                var lockoutEnd = await manager.GetLockoutEndDateAsync(user);
                if (lockoutEnd.HasValue)
                {
                    var remainingTime = lockoutEnd.Value - DateTimeOffset.UtcNow;
                    return IdentityResult.Failed(new IdentityError
                    {
                        Code = "UserLockedOut",
                        Description = $"Account is locked. Please try again in {remainingTime.Minutes} minutes."
                    });
                }
            }

            // Track failed attempts
            var failedCount = await manager.GetAccessFailedCountAsync(user);
            if (failedCount >= 3)
            {
                await manager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(5));
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "AccountLocked",
                    Description = "Account locked due to multiple failed attempts. Please try again in 5 minutes."
                });
            }

            return IdentityResult.Success;
        }
    }
}
