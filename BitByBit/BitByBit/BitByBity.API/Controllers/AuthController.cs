using BitByBit.Business.DTOs.Auth;
using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.User;
using BitByBit.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace BitByBity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IUserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// İstifadəçi qeydiyyatı və email confirmation code göndərmə
        /// </summary>
        /// <param name="registerDto">Qeydiyyat məlumatları</param>
        /// <returns>Qeydiyyat nəticəsi</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
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

                var result = await _userService.RegisterAsync(registerDto);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { Email = registerDto.Email },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for email: {Email}", registerDto.Email);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// İstifadəçi girişi və JWT token qaytarma
        /// </summary>
        /// <param name="loginDto">Giriş məlumatları</param>
        /// <returns>Login nəticəsi və JWT token</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
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

                var result = await _userService.LoginAsync(loginDto);

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
                _logger.LogError(ex, "Error during login for email: {Email}", loginDto.Email);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// İstifadəçi çıxışı - Cookie təmizləmə və session invalidate
        /// </summary>
        /// <returns>Logout nəticəsi</returns>
        [HttpPost("logout")]
        [Authorize] // Authentication tələb edir
        public async Task<IActionResult> Logout()
        {
            try
            {
                // JWT token-dən user ID al
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _userService.LogoutAsync(userId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { UserId = userId, LogoutTime = DateTime.Now },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Bütün cihazlardan çıxış - Security stamp update
        /// </summary>
        /// <returns>Global logout nəticəsi</returns>
        [HttpPost("logout-all")]
        [Authorize]
        public async Task<IActionResult> LogoutFromAllDevices()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _userService.LogoutFromAllDevicesAsync(userId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new
                        {
                            UserId = userId,
                            LogoutTime = DateTime.Now,
                            Type = "Global Logout"
                        },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during global logout");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Email təsdiq kodu göndərmə (yenidən göndərmə)
        /// </summary>
        /// <param name="email">Email ünvanı</param>
        /// <returns>Code göndərmə nəticəsi</returns>
        [HttpPost("send-confirmation-code")]
        public async Task<IActionResult> SendConfirmationCode([FromBody] string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Email tələb olunur"));
                }

                var result = await _userService.SendEmailConfirmationCodeAsync(email);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { Email = email },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending confirmation code to: {Email}", email);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Email təsdiq kodu ilə hesab təsdiqi
        /// </summary>
        /// <param name="confirmEmailDto">Email və təsdiq kodu</param>
        /// <returns>Təsdiq nəticəsi</returns>
        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailDto confirmEmailDto)
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

                var result = await _userService.ConfirmEmailAsync(confirmEmailDto);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { Email = confirmEmailDto.Email },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error confirming email: {Email}", confirmEmailDto.Email);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Şifrə sıfırlama kodu göndərmə
        /// </summary>
        /// <param name="forgotPasswordDto">Email ünvanı</param>
        /// <returns>Reset code göndərmə nəticəsi</returns>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
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

                var result = await _userService.SendPasswordResetCodeAsync(forgotPasswordDto);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { Email = forgotPasswordDto.Email },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending password reset code to: {Email}", forgotPasswordDto.Email);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Şifrə sıfırlama kodu ilə yeni şifrə qoyma
        /// </summary>
        /// <param name="resetPasswordDto">Reset code və yeni şifrə</param>
        /// <returns>Şifrə sıfırlama nəticəsi</returns>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
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

                var result = await _userService.ResetPasswordAsync(resetPasswordDto);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { Email = resetPasswordDto.Email },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message, result.Errors));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for: {Email}", resetPasswordDto.Email);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Refresh token ilə yeni access token almaq
        /// </summary>
        /// <param name="refreshTokenDto">Access token və refresh token</param>
        /// <returns>Yeni token pair</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
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

                var result = await _userService.RefreshTokenAsync(refreshTokenDto);

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
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Refresh token revoke etmə
        /// </summary>
        /// <param name="revokeTokenDto">Revoke ediləcək refresh token</param>
        /// <returns>Token revoke nəticəsi</returns>
        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RevokeTokenDto revokeTokenDto)
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

                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                // Future implementation - database-də refresh token revoke
                return Ok(ApiResponse.SuccessResponse(
                    data: new { UserId = userId, RevokedToken = revokeTokenDto.RefreshToken },
                    message: "Refresh token revoke edildi"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Email mövcudluq yoxlaması
        /// </summary>
        /// <param name="email">Email ünvanı</param>
        /// <returns>Email mövcudluq nəticəsi</returns>
        [HttpGet("check-email/{email}")]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Email tələb olunur"));
                }

                var exists = await _userService.IsEmailExistsAsync(email);

                return Ok(ApiResponse.SuccessResponse(
                    data: new { Email = email, Exists = exists },
                    message: exists ? "Email mövcuddur" : "Email mövcud deyil"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email existence: {Email}", email);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }
    }

    // Supporting DTOs
    public class RevokeTokenDto
    {
        [Required(ErrorMessage = "Refresh token tələb olunur")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}