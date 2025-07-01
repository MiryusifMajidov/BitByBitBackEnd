using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Entities.Constants
{
    public static class ErrorMessages
    {
        // User Errors
        public const string UserNotFound = "İstifadəçi tapılmadı";
        public const string UserAlreadyExists = "Bu email artıq istifadə olunur";
        public const string InvalidCredentials = "Email və ya şifrə yanlışdır";
        public const string UserBanned = "İstifadəçi hesabı bloklanıb";
        public const string UserInactive = "İstifadəçi hesabı aktiv deyil";
        public const string EmailNotConfirmed = "Email təsdiqlənməyib";

        // Validation Errors
        public const string EmailRequired = "Email tələb olunur";
        public const string EmailInvalid = "Email formatı yanlışdır";
        public const string PasswordRequired = "Şifrə tələb olunur";
        public const string PasswordTooShort = "Şifrə minimum 6 simvol olmalıdır";
        public const string FirstNameRequired = "Ad tələb olunur";
        public const string LastNameRequired = "Soyad tələb olunur";

        // General Errors
        public const string SomethingWentWrong = "Xəta baş verdi, yenidən cəhd edin";
        public const string UnauthorizedAccess = "Bu əməliyyat üçün icazəniz yoxdur";
        public const string DatabaseError = "Verilənlər bazası xətası";
    }
}
