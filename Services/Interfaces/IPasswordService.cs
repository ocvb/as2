namespace _234412H_AS2.Services.Interfaces
{
    public interface IPasswordService
    {
        Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword);
    }
}
