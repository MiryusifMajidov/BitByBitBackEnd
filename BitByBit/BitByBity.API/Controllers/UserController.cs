// UserController.cs - BitByBit.API/Controllers/UserController.cs

using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.User;
using BitByBit.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BitByBit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Bütün endpoint-lər authentication tələb edir
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Current user-in profil məlumatlarını əldə etmə
        /// </summary>
        /// <returns>User profil məlumatları</returns>
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _userService.GetUserProfileAsync(userId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: "Profil məlumatları əldə edildi"
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user profile");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Current user-in profil məlumatlarını yeniləmə
        /// </summary>
        /// <param name="updateProfileDto">Yenilənəcək məlumatlar</param>
        /// <returns>Yenilənmiş profil məlumatları</returns>
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse.ErrorResponse("Validation xətaları", errors));
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _userService.UpdateUserProfileAsync(userId, updateProfileDto);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user profile");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Current user-in şifrəsini dəyişmə
        /// </summary>
        /// <param name="changePasswordDto">Cari və yeni şifrə</param>
        /// <returns>Şifrə dəyişmə nəticəsi</returns>
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponse.ErrorResponse("Validation xətaları", errors));
                }

                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _userService.ChangePasswordAsync(userId, changePasswordDto);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { UserId = userId },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Current user-in hesabını deaktivləşdirmə/aktivləşdirmə
        /// </summary>
        /// <param name="isActive">True = Aktiv, False = Deaktiv</param>
        /// <returns>Status dəyişmə nəticəsi</returns>
        [HttpPatch("toggle-status")]
        public async Task<IActionResult> ToggleUserStatus([FromBody] bool isActive)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _userService.ToggleUserStatusAsync(userId, isActive);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { UserId = userId, IsActive = isActive },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling user status");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Current user-in hesabını silmə (Soft Delete)
        /// </summary>
        /// <returns>Hesab silmə nəticəsi</returns>
        [HttpDelete("account")]
        public async Task<IActionResult> DeleteAccount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _userService.DeleteUserAccountAsync(userId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { UserId = userId },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user account");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Email ilə user məlumatlarını əldə etmə (Admin üçün)
        /// </summary>
        /// <param name="email">Email ünvanı</param>
        /// <returns>User məlumatları</returns>
        [HttpGet("by-email/{email}")]
        [Authorize(Roles = "Admin")] // Yalnız Admin-lər istifadə edə bilər
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Email tələb olunur"));
                }

                var result = await _userService.GetUserByEmailAsync(email);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: "İstifadəçi məlumatları əldə edildi"
                    ));
                }

                return NotFound(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by email: {Email}", email);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// JWT Token-dən current user ID-ni əldə etmə
        /// </summary>
        /// <returns>User ID və ya null</returns>
        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        /// <summary>
        /// JWT Token-dən current user email-ni əldə etmə
        /// </summary>
        /// <returns>User email və ya null</returns>
        private string? GetCurrentUserEmail()
        {
            return User.FindFirst(ClaimTypes.Email)?.Value;
        }

        /// <summary>
        /// Current user-in admin olub-olmadığını yoxlama
        /// </summary>
        /// <returns>True əgər admin-dirsə</returns>
        private bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Admin");
        }

        #endregion
    }
}