using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _234412H_AS2.Pages.Errors
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class _404Model : PageModel
    {
        private readonly ILogger<_404Model> _logger;

        public _404Model(ILogger<_404Model> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogWarning("404 error occurred. Path: {Path}", HttpContext.Request.Path);
            Response.StatusCode = 404;
        }
    }
}
