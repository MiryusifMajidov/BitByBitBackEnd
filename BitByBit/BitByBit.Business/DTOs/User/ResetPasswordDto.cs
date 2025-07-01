using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.DTOs.User
{
    public class ResetPasswordDto
    {
        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress(ErrorMessage = "Email formatı yanlışdır")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Sıfırlama kodu tələb olunur")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "Sıfırlama kodu 4 rəqəm olmalıdır")]
        public string ResetCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yeni şifrə tələb olunur")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifrə minimum 6, maksimum 100 simvol olmalıdır")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifrə təkrarı tələb olunur")]
        [Compare("NewPassword", ErrorMessage = "Şifrələr uyğun gəlmir")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
