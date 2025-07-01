using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.DTOs.Auth
{
    public class RefreshTokenDto
    {
        [Required(ErrorMessage = "Access token tələb olunur")]
        public string AccessToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Refresh token tələb olunur")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
