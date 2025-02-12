using Microsoft.AspNetCore.Identity;

namespace _234412H_AS2.Model
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = "";
        public string CreditCardNumber { get; set; } = "";
        public string Gender { get; set; } = "";
        public string Address { get; set; } = "";
        public string? ProfilePicture { get; set; } = "";
        public string? AboutMe { get; set; } = "";

    }
}
