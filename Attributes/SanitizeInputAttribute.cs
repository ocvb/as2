using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace _234412H_AS2.Attributes
{
    public class SanitizeInputAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            string stringValue = value.ToString()!;

            // Check for potential XSS patterns
            if (Regex.IsMatch(stringValue, @"<script|javascript:|data:|vbscript:", RegexOptions.IgnoreCase))
            {
                return new ValidationResult("Invalid characters detected in input.");
            }

            // Check for SQL injection patterns
            if (Regex.IsMatch(stringValue, @"(?i)(select|insert|update|delete|drop|truncate|alter|exec)\s|(\-\-)|(/\*.*\*/)|(\b(all|any|not|and|between|in|like|or|some|contains|containsall|containskey)\b.*[=><])", RegexOptions.IgnoreCase))
            {
                return new ValidationResult("Invalid characters detected in input.");
            }

            return ValidationResult.Success;
        }
    }
}
