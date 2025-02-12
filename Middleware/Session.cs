using _234412H_AS2.Services;

namespace _234412H_AS2.Middleware
{
    public class SessionMiddleware(RequestDelegate next, ISessionService sessionService)
    {
        private readonly RequestDelegate _next = next;
        private readonly ISessionService _sessionService = sessionService;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User.Identity.IsAuthenticated)
            {
                var sessionId = _sessionService.GetSessionId();
                if (string.IsNullOrEmpty(sessionId))
                {
                    context.Response.Redirect("/");
                    return;
                }
            }
            await _next(context);
        }
    }
}
