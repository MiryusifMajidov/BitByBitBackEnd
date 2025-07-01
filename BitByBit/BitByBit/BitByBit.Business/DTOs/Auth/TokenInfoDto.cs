using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.DTOs.Auth
{
    public class TokenInfoDto
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public bool IsExpired { get; set; }
        public List<ClaimInfo> Claims { get; set; } = new();
    }

    public class ClaimInfo
    {
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
