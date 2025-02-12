using System.Threading.Tasks;

namespace _234412H_AS2.Services.Interfaces
{
    public interface IPasswordChangeService
    {
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
        Task<bool> ValidatePasswordAsync(string userId, string password);
    }
}
