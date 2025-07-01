using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.DTOs.User
{
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Ad tələb olunur")]
        [StringLength(50, ErrorMessage = "Ad maksimum 50 simvol ola bilər")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad tələb olunur")]
        [StringLength(50, ErrorMessage = "Soyad maksimum 50 simvol ola bilər")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefon nömrəsi formatı yanlışdır")]
        public string? PhoneNumber { get; set; }
    }
}
