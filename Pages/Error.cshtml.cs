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

        public void OnGet(int? statusCode)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            StatusCode = statusCode ?? 500;


            ErrorMessage = StatusCode switch
            {
                400 => "Bad Request - The server cannot process your request.",
                401 => "Unauthorized - You need to authenticate first.",
                403 => "Forbidden - You don't have permission to access this resource.",
                404 => "Page Not Found - The requested page does not exist.",
                500 => "Internal Server Error - Something went wrong on our end.",
                _ => "An error occurred while processing your request."
            };

            Response.StatusCode = StatusCode;
            _logger.LogError("Error {StatusCode}: {ErrorMessage} - RequestId: {RequestId}",
                StatusCode, ErrorMessage, RequestId);
        }
    }
}
