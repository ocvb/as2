using System;

namespace _234412H_AS2.Model
{
    public class PasswordPolicyOptions
    {
        public TimeSpan MinPasswordAge { get; set; }
        public TimeSpan MaxPasswordAge { get; set; }
        public int PasswordHistoryLimit { get; set; }
    }
}
