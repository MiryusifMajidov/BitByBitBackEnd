using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.DTOs.User
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Ad tələb olunur")]
        [StringLength(50, ErrorMessage = "Ad maksimum 50 simvol ola bilər")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad tələb olunur")]
        [StringLength(50, ErrorMessage = "Soyad maksimum 50 simvol ola bilər")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress(ErrorMessage = "Email formatı yanlışdır")]
        [StringLength(100, ErrorMessage = "Email maksimum 100 simvol ola bilər")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə tələb olunur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifrə minimum 6, maksimum 100 simvol olmalıdır")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə təkrarı tələb olunur")]
        [Compare("Password", ErrorMessage = "Şifrələr uyğun gəlmir")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefon nömrəsi formatı yanlışdır")]
        public string? PhoneNumber { get; set; }
    }
}
