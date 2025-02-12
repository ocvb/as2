using System;

namespace _234412H_AS2.Model
{
    public class TwoFactorAuth
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string SecretKey { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime EnabledDate { get; set; }
        public virtual ApplicationUser User { get; set; }
    }
}
