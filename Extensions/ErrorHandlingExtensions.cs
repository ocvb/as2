using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace _234412H_AS2.Extensions
{
    public static class ErrorHandlingExtensions
    {
        public static IActionResult HandleError(this PageModel page, string message)
        {
            page.TempData["ErrorMessage"] = message;
            return page.RedirectToPage("/Error");
        }
    }
}
