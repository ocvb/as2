using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using _234412H_AS2.Model;
using _234412H_AS2.Services;

namespace _234412H_AS2.Pages
{
    public class LogoutModel(SignInManager<ApplicationUser> signInManager,
        ISessionService sessionService) : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly ISessionService _sessionService = sessionService;

        public async Task<IActionResult> OnPost()
        {
            await _signInManager.SignOutAsync();
            _sessionService.ClearSession();
            return RedirectToPage("/Login");
        }
    }
}
