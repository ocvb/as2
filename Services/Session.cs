

namespace _234412H_AS2.Services
{
    public interface ISessionService
    {
        string GenerateSessionId();
        void SetSessionId(string sessionId);
        string GetSessionId();
        void ClearSession();
        void SetUserData(string key, string value);
        string GetUserData(string key);
    }

    public class SessionService(IHttpContextAccessor httpContextAccessor) : ISessionService
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private const string SessionKey = "UserSessionId";          

        public string GenerateSessionId() => Guid.NewGuid().ToString();

        public void SetSessionId(string sessionId)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(SessionKey, sessionId);
        }

        public string GetSessionId()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(SessionKey);
        }

        public void ClearSession()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
        }

        public void SetUserData(string key, string value)
        {
            _httpContextAccessor.HttpContext?.Session.SetString(key, value);
        }

        public string GetUserData(string key)
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(key);
        }
    }
}
