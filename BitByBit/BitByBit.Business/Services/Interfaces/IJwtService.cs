using BitByBit.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.Services.Interfaces
{
    public interface IJwtService
    {
        #region Token Generation

        /// <summary>
        /// Generate JWT access token for user
        /// </summary>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Generate JWT refresh token
        /// </summary>
        string GenerateRefreshToken();

        /// <summary>
        /// Generate token with custom claims
        /// </summary>
        string GenerateToken(IEnumerable<Claim> claims);

        #endregion

        #region Token Validation

        /// <summary>
        /// Validate JWT token
        /// </summary>
        ClaimsPrincipal? ValidateToken(string token);

        /// <summary>
        /// Check if token is expired
        /// </summary>
        bool IsTokenExpired(string token);

        /// <summary>
        /// Get token expiry date
        /// </summary>
        DateTime? GetTokenExpiry(string token);

        #endregion

        #region Token Extraction

        /// <summary>
        /// Extract user ID from token
        /// </summary>
        string? GetUserIdFromToken(string token);

        /// <summary>
        /// Extract email from token
        /// </summary>
        string? GetEmailFromToken(string token);

        /// <summary>
        /// Extract role from token
        /// </summary>
        string? GetRoleFromToken(string token);

        /// <summary>
        /// Extract all claims from token
        /// </summary>
        IEnumerable<Claim> GetClaimsFromToken(string token);

        #endregion

        #region Refresh Token

        /// <summary>
        /// Refresh access token using refresh token
        /// </summary>
        Task<(string AccessToken, string RefreshToken)> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Revoke refresh token
        /// </summary>
        Task RevokeRefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Validate refresh token
        /// </summary>
        bool ValidateRefreshToken(string refreshToken);

        #endregion

        #region Security

        /// <summary>
        /// Generate secure random string
        /// </summary>
        string GenerateSecureRandomString(int length = 32);

        /// <summary>
        /// Hash refresh token for storage
        /// </summary>
        string HashRefreshToken(string refreshToken);

        #endregion
    }
}