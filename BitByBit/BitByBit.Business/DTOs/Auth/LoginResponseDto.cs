using BitByBit.Business.DTOs.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;  // ← Yeni əlavə edildi
        public DateTime Expires { get; set; }
        public string TokenType { get; set; } = "Bearer";  // ← Yeni əlavə edildi
        public UserResponseDto User { get; set; } = new();
    }
}
