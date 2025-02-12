using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace _234412H_AS2.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<bool> Enable2faAsync(string userId);
        Task<bool> Disable2faAsync(string userId);
        Task<string> Generate2faTokenAsync(string userId);
        Task<bool> Verify2faTokenAsync(string userId, string token);
        Task<bool> GetTwoFactorEnabledAsync(string userId);
        Task<(byte[] qrCodeImage, string manualEntryKey)> Generate2faSetupInfoAsync(string userId, string email);
    }
}
