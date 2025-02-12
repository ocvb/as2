using _234412H_AS2.Model;
using Microsoft.AspNetCore.Http;

namespace _234412H_AS2.Services
{
    public class AuditService
    {
        private readonly AuthContextDb _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditService(AuthContextDb context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAction(string userId, string action, string details)
        {
            var context = _httpContextAccessor.HttpContext;
            var auditLog = new AuditLog
            {
                UserId = userId,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow,
                IpAddress = context?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                UserAgent = context?.Request?.Headers.UserAgent.ToString() ?? "Unknown"
            };

            _context.AuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();
        }
    }
}
