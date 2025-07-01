using BitByBit.Business.DTOs.Auth;
using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.User;
using BitByBit.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.Services.Interfaces
{
    public interface IUserService
    {
        #region Authentication
        /// <summary>
        /// İstifadəçi qeydiyyatı və email confirmation code göndərmə
        /// </summary>
        Task<ServiceResult> RegisterAsync(RegisterDto registerDto);

        /// <summary>
        /// İstifadəçi girişi və JWT token qaytarma
        /// </summary>
        Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto loginDto);

        /// <summary>
        /// İstifadəçi çıxışı - Cookie təmizləmə və token revoke
        /// </summary>
        Task<ServiceResult> LogoutAsync(string userId);

        /// <summary>
        /// Global logout - bütün cihazlardan çıxış
        /// </summary>
        Task<ServiceResult> LogoutFromAllDevicesAsync(string userId);
        #endregion</thinking>



        #region JWT Token Management
        /// <summary>
        /// Refresh token ilə yeni access token almaq
        /// </summary>
        Task<ServiceResult<TokenRefreshResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);

        /// <summary>
        /// Token validation və user məlumatları almaq
        /// </summary>
        Task<ServiceResult<User>> ValidateTokenAndGetUserAsync(string token);
        #endregion

        #region Email Confirmation
        /// <summary>
        /// Email təsdiq kodu göndərmə (ilk qeydiyyat və ya yenidən göndərmə)
        /// </summary>
        Task<ServiceResult> SendEmailConfirmationCodeAsync(string email);

        /// <summary>
        /// Email təsdiq kodu ilə hesab təsdiqi
        /// </summary>
        Task<ServiceResult> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto);
        #endregion

        #region Password Reset
        /// <summary>
        /// Şifrə sıfırlama kodu göndərmə
        /// </summary>
        Task<ServiceResult> SendPasswordResetCodeAsync(ForgotPasswordDto forgotPasswordDto);

        /// <summary>
        /// Şifrə sıfırlama kodu ilə yeni şifrə qoyma
        /// </summary>
        Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
        #endregion

        #region Password Management
        /// <summary>
        /// Cari şifrə ilə yeni şifrə dəyişmə
        /// </summary>
        Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto);
        #endregion

        #region Profile Management
        /// <summary>
        /// İstifadəçi profili əldə etmə
        /// </summary>
        Task<ServiceResult<UserResponseDto>> GetUserProfileAsync(string userId);

        Task<ServiceResult<List<UserResponseDto>>> GetAllUsersAsync();
        Task<ServiceResult<UserResponseDto>> UpdateUserProfileAsync(string userId, UpdateProfileDto updateProfileDto);

        /// <summary>
        /// Email ilə istifadəçi tapma
        /// </summary>
        Task<ServiceResult<UserResponseDto>> GetUserByEmailAsync(string email);
        #endregion

        #region Account Management
        /// <summary>
        /// Hesab aktivləşdirmə/deaktivləşdirmə
        /// </summary>
        Task<ServiceResult> ToggleUserStatusAsync(string userId, bool isActive);

        /// <summary>
        /// İstifadəçi hesabını soft delete
        /// </summary>
        Task<ServiceResult> DeleteUserAccountAsync(string userId);
        #endregion

        #region Validation Helpers
        /// <summary>
        /// Email mövcudluq yoxlaması
        /// </summary>
        Task<bool> IsEmailExistsAsync(string email);

        /// <summary>
        /// İstifadəçi mövcudluq yoxlaması
        /// </summary>
        Task<bool> IsUserExistsAsync(string userId);
        #endregion
    }
}