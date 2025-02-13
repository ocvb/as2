using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _234412H_AS2.Pages.Error
{
    public class ErrorModel : PageModel
    {
        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string ErrorMessage { get; set; }

        public void OnGet(string message = null)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            ErrorMessage = message;
        }
    }
}
