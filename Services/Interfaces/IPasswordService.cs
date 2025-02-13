namespace _234412H_AS2.Services.Interfaces
{
    public interface IPasswordService
    {
        Task<(bool success, string message)> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
