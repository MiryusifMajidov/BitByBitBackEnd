using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.DTOs.User
{
    public class ConfirmEmailDto
    {
        [Required(ErrorMessage = "Email tələb olunur")]
        [EmailAddress(ErrorMessage = "Email formatı yanlışdır")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Təsdiq kodu tələb olunur")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Təsdiq kodu 6 rəqəm olmalıdır")]
        public string ConfirmationCode { get; set; } = string.Empty;
    }
}
