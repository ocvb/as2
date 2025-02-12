using Microsoft.AspNetCore.Mvc.Filters;
using _234412H_AS2.Services;

namespace _234412H_AS2.Attributes
{
    public class ValidateSessionAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var sessionService = context.HttpContext.RequestServices.GetService<ISessionService>();
            var sessionId = sessionService?.GetSessionId();

            if (string.IsNullOrEmpty(sessionId))
            {
                context.HttpContext.Response.Redirect("/Login");
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}
