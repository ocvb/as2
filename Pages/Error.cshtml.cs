using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _234412H_AS2.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public string? RequestId { get; set; }
        public string? ErrorMessage { get; set; }
        public new int StatusCode { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string PageTitle { get; set; }

        public IActionResult OnGet(int? statusCode)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            StatusCode = statusCode ?? 500;

            (ErrorMessage, PageTitle) = StatusCode switch
            {
                400 => ("Your request contains invalid parameters or formatting.", "Bad Request"),
                401 => ("Please log in to access this resource.", "Authentication Required"),
                403 => ("You don't have sufficient permissions to access this resource.", "Access Denied"),
                404 => ("The page or resource you're looking for could not be found.", "Page Not Found"),
                500 => ("We're experiencing technical difficulties. Please try again later.", "Server Error"),
                _ => ("An unexpected error occurred while processing your request.", "Error")
            };

            Response.StatusCode = StatusCode;

            // Log with additional context
            var logMessage = $"Error occurred - Status: {StatusCode}, URL: {Request.Path}, Method: {Request.Method}";
            _logger.LogError("{Message}. RequestId: {RequestId}, ErrorMessage: {ErrorMessage}",
                logMessage, RequestId, ErrorMessage);

            // Redirect to specific error pages for common scenarios
            return StatusCode switch
            {
                404 => RedirectToPage("/Error/404"),
                403 => RedirectToPage("/Error/403"),
                500 => RedirectToPage("/Error/500"),
                _ => Page()
            };
        }
    }
}
