using System.ComponentModel.DataAnnotations;
using _234412H_AS2.Attributes;


namespace _234412H_AS2.ViewModels
{
    public class Register
    {
        [Required]
        [Display(Name = "Full Name")]
        [DataType(DataType.Text)]
        [SanitizeInput]
        public string FullName { get; set; } = "";

        [Required]
        [Display(Name = "Credit Card Number")]
        [DataType(DataType.CreditCard)]
        [SanitizeInput]
        public string CreditCardNumber { get; set; } = "";

        [Required]
        [Display(Name = "Gender")]
        [DataType(DataType.Text)]
        [SanitizeInput]
        public string Gender { get; set; } = "";

        [Required]
        [Display(Name = "Phone Number")]
        [DataType(DataType.PhoneNumber)]
        [SanitizeInput]
        public string Phone { get; set; } = "";

        [Required]
        [Display(Name = "Address")]
        [DataType(DataType.Text)]
        [SanitizeInput]
        public string Address { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        [SanitizeInput]
        public string Email { get; set; } = "";

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 12)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{12,}$", ErrorMessage = "Password must be at least 12 characters long and contain: upper case (A-Z), lower case (a-z), number (0-9) and special character (e.g. !@#$%^&*)")]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = "";

        [DataType(DataType.Upload)]
        public IFormFile? ProfilePicture { get; set; }

        [DataType(DataType.Text)]
        [SanitizeInput]
        public string? AboutMe { get; set; }

    }



}
