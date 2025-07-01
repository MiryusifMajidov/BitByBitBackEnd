using BitByBit.Business.Services.Interfaces;
using BitByBit.Core.Models;
using BitByBit.Entities.Models;
using BitByBit.Entities.Enums;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BitByBit.Business.Services.Implementations
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly JwtSecurityTokenHandler _tokenHandler;
        private readonly byte[] _secretKey;

        public JwtService(IOptions<JwtSettings> jwtSettings)
        {
            _jwtSettings = jwtSettings.Value;
            _tokenHandler = new JwtSecurityTokenHandler();
            _secretKey = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
        }

        #region Token Generation

        public string GenerateAccessToken(User user)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Name, user.UserName ?? user.Email),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                new(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new("status", user.Status.ToString()),
                new("emailConfirmed", user.EmailConfirmed.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Aud, _jwtSettings.Audience),
                new(JwtRegisteredClaimNames.Iss, _jwtSettings.Issuer)
            };

            // ✅ ROLE PROBLEM HƏLLİ - enum nullable deyil
            claims.Add(new(ClaimTypes.Role, user.Role.ToString())); // "Admin", "User", "Moderator"

            // ✅ Əlavə role claim-ləri
            claims.Add(new("roleId", ((int)user.Role).ToString())); // "1", "2", "3"
            claims.Add(new("roleName", user.Role.ToString())); // "Admin", "User", "Moderator"

            // Add phone number if exists
            if (!string.IsNullOrEmpty(user.PhoneNumber))
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            }

            return GenerateToken(claims);
        }

        public string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        public string GenerateToken(IEnumerable<Claim> claims)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_secretKey),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                NotBefore = DateTime.UtcNow,
                IssuedAt = DateTime.UtcNow
            };

            var token = _tokenHandler.CreateToken(tokenDescriptor);
            return _tokenHandler.WriteToken(token);
        }

        #endregion

        #region Token Validation

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = _jwtSettings.ValidateIssuer,
                    ValidateAudience = _jwtSettings.ValidateAudience,
                    ValidateLifetime = _jwtSettings.ValidateLifetime,
                    ValidateIssuerSigningKey = _jwtSettings.ValidateIssuerSigningKey,
                    RequireExpirationTime = _jwtSettings.RequireExpirationTime,
                    ValidIssuer = _jwtSettings.Issuer,
                    ValidAudience = _jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(_secretKey),
                    ClockSkew = TimeSpan.FromMinutes(_jwtSettings.ClockSkewMinutes)
                };

                var principal = _tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                if (validatedToken is JwtSecurityToken jwtToken &&
                    jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return principal;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public bool IsTokenExpired(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo <= DateTime.UtcNow;
            }
            catch
            {
                return true;
            }
        }

        public DateTime? GetTokenExpiry(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.ValidTo;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Token Extraction

        public string? GetUserIdFromToken(string token)
        {
            var claims = GetClaimsFromToken(token);
            return claims?.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetEmailFromToken(string token)
        {
            var claims = GetClaimsFromToken(token);
            return claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        }

        public string? GetRoleFromToken(string token)
        {
            var claims = GetClaimsFromToken(token);
            return claims?.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
        }

        /// <summary>
        /// ✅ Role ID-ni token-dən əldə et
        /// </summary>
        public int? GetRoleIdFromToken(string token)
        {
            var claims = GetClaimsFromToken(token);
            var roleIdClaim = claims?.FirstOrDefault(c => c.Type == "roleId")?.Value;
            return int.TryParse(roleIdClaim, out var roleId) ? roleId : null;
        }

        /// <summary>
        /// ✅ UserRole enum-u token-dən əldə et  
        /// </summary>
        public UserRole? GetUserRoleFromToken(string token)
        {
            var roleId = GetRoleIdFromToken(token);
            if (roleId.HasValue && Enum.IsDefined(typeof(UserRole), roleId.Value))
            {
                return (UserRole)roleId.Value;
            }
            return null;
        }

        public IEnumerable<Claim> GetClaimsFromToken(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return jwtToken.Claims;
            }
            catch
            {
                return Enumerable.Empty<Claim>();
            }
        }

        #endregion

        #region Refresh Token

        public async Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken)
        {
            // In a real application, you would:
            // 1. Validate refresh token against database
            // 2. Get user from database using refresh token
            // 3. Generate new access token and refresh token
            // 4. Update refresh token in database
            // 5. Return new tokens

            // For now, return empty - implement when you add refresh token storage
            await Task.CompletedTask;
            throw new NotImplementedException("Refresh token functionality requires database storage implementation");
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            // In a real application, you would:
            // 1. Mark refresh token as revoked in database
            // 2. Optionally revoke all refresh tokens for user

            await Task.CompletedTask;
            throw new NotImplementedException("Refresh token revocation requires database storage implementation");
        }

        public bool ValidateRefreshToken(string refreshToken)
        {
            // In a real application, you would:
            // 1. Check if refresh token exists in database
            // 2. Check if it's not expired
            // 3. Check if it's not revoked

            throw new NotImplementedException("Refresh token validation requires database storage implementation");
        }

        #endregion

        #region Security

        public string GenerateSecureRandomString(int length = 32)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            var result = new char[length];

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        public string HashRefreshToken(string refreshToken)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(hashedBytes);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Get token info for debugging - ✅ ENHANCED
        /// </summary>
        public object GetTokenInfo(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                var claims = jwtToken.Claims.ToList();

                return new
                {
                    Header = jwtToken.Header,
                    Payload = jwtToken.Payload,
                    ValidFrom = jwtToken.ValidFrom,
                    ValidTo = jwtToken.ValidTo,
                    IsExpired = jwtToken.ValidTo <= DateTime.UtcNow,
                    UserId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value,
                    Email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value,
                    Role = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value,
                    RoleId = claims.FirstOrDefault(c => c.Type == "roleId")?.Value,
                    RoleName = claims.FirstOrDefault(c => c.Type == "roleName")?.Value,
                    AllClaims = claims.Select(c => new { c.Type, c.Value }).ToList()
                };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        /// <summary>
        /// Decode token without validation (for debugging)
        /// </summary>
        public object DecodeToken(string token)
        {
            try
            {
                var jwtToken = _tokenHandler.ReadJwtToken(token);
                return new
                {
                    jwtToken.Header,
                    Payload = jwtToken.Claims.ToDictionary(c => c.Type, c => c.Value),
                    jwtToken.ValidFrom,
                    jwtToken.ValidTo
                };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        #endregion
    }
}