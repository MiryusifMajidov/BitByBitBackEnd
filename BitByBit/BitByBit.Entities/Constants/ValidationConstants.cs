using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Entities.Constants
{
    public static class ValidationConstants
    {
        // String Lengths
        public const int FirstNameMaxLength = 50;
        public const int LastNameMaxLength = 50;
        public const int EmailMaxLength = 100;
        public const int PhoneNumberMaxLength = 20;
        public const int PasswordMinLength = 6;
        public const int PasswordMaxLength = 100;

        // Password Requirements
        public const string PasswordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{6,}$";
        public const string PasswordRequirements = "Şifrə minimum 6 simvol olmalı, ən azı bir böyük hərf, bir kiçik hərf və bir rəqəm olmalıdır";

        // Email Pattern
        public const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        // Phone Pattern
        public const string PhonePattern = @"^(\+994|0)(50|51|55|70|77|99)[0-9]{7}$";
        public const string PhoneRequirements = "Telefon nömrəsi Azərbaycan formatında olmalıdır (+994xxxxxxxxx)";

        // Token Expiry
        public const int EmailConfirmTokenExpiryMinutes = 1440; // 24 hours
        public const int ResetPasswordTokenExpiryMinutes = 30;  // 30 minutes
        public const int JwtTokenExpiryMinutes = 60; // 1 hour
    }
}
