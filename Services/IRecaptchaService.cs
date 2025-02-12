namespace _234412H_AS2.Services
{
    public interface IRecaptchaService
    {
        Task<bool> VerifyToken(string token);
    }
}
