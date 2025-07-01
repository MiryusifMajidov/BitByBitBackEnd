using AutoMapper;
using BitByBit.Business.DTOs.Auth;
using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.User;
using BitByBit.Business.Helpers;
using BitByBit.Business.Services.Interfaces;
using BitByBit.Entities.Constants;
using BitByBit.Entities.Enums;
using BitByBit.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BitByBit.Business.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;

     
        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expiry)> _confirmationCodes = new();
        private static readonly ConcurrentDictionary<string, (string Code, DateTime Expiry)> _resetCodes = new();

        public UserService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IEmailService emailService,
            IJwtService jwtService,
            IMapper mapper,
            ILogger<UserService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _jwtService = jwtService;
            _mapper = mapper;
            _logger = logger;
        }

        #region Authentication

        public async Task<ServiceResult> RegisterAsync(RegisterDto registerDto)
        {
            try
            {
           
                if (await IsEmailExistsAsync(registerDto.Email))
                {
                    return ServiceResult.ErrorResult(ErrorMessages.UserAlreadyExists);
                }

                var user = new User
                {
                    UserName = registerDto.Email,
                    Email = registerDto.Email,
                    FirstName = registerDto.FirstName,
                    LastName = registerDto.LastName,
                    PhoneNumber = registerDto.PhoneNumber,
                    Role = UserRole.User,
                    Status = UserStatus.Active,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong, errors);
                }

            
                await SendEmailConfirmationCodeAsync(registerDto.Email);

                _logger.LogInformation($"User registered successfully: {registerDto.Email}");
                return ServiceResult.SuccessResult(SuccessMessages.UserCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error registering user: {registerDto.Email}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        public async Task<ServiceResult<LoginResponseDto>> LoginAsync(LoginDto loginDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginDto.Email);
                if (user == null)
                {
                    return ServiceResult<LoginResponseDto>.ErrorResult(ErrorMessages.InvalidCredentials);
                }

                if (user.Status == UserStatus.Banned)
                {
                    return ServiceResult<LoginResponseDto>.ErrorResult(ErrorMessages.UserBanned);
                }

                if (user.Status == UserStatus.Inactive)
                {
                    return ServiceResult<LoginResponseDto>.ErrorResult(ErrorMessages.UserInactive);
                }

                if (!user.EmailConfirmed)
                {
                    return ServiceResult<LoginResponseDto>.ErrorResult(ErrorMessages.EmailNotConfirmed);
                }

                var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: true);

                if (!result.Succeeded)
                {
                    if (result.IsLockedOut)
                    {
                        return ServiceResult<LoginResponseDto>.ErrorResult("Hesab müvəqqəti bloklanıb. Sonra yenidən cəhd edin.");
                    }

                    return ServiceResult<LoginResponseDto>.ErrorResult(ErrorMessages.InvalidCredentials);
                }

              
                user.LastLoginDate = DateTime.Now;
                await _userManager.UpdateAsync(user);

                var accessToken = _jwtService.GenerateAccessToken(user);
                var refreshToken = _jwtService.GenerateRefreshToken();

                var tokenExpiry = DateTime.Now.AddHours(24);

                var userResponse = _mapper.Map<UserResponseDto>(user);
                var loginResponse = new LoginResponseDto
                {
                    Token = accessToken,
                    RefreshToken = refreshToken,
                    Expires = tokenExpiry,
                    TokenType = "Bearer",
                    User = userResponse
                };

                _logger.LogInformation($"User logged in successfully with JWT: {loginDto.Email}");
                return ServiceResult<LoginResponseDto>.SuccessResult(loginResponse, SuccessMessages.LoginSuccessful);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during login: {loginDto.Email}");
                return ServiceResult<LoginResponseDto>.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        public async Task<ServiceResult> LogoutAsync(string userId)
        {
            try
            {
                // User mövcudluq yoxlaması
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult.ErrorResult(ErrorMessages.UserNotFound);
                }

                await _signInManager.SignOutAsync();

                await _userManager.UpdateSecurityStampAsync(user);

                _logger.LogInformation($"User logged out successfully: {user.Email}");

                return ServiceResult.SuccessResult(SuccessMessages.LogoutSuccessful);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during logout for user: {userId}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        public async Task<ServiceResult> LogoutFromAllDevicesAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult.ErrorResult(ErrorMessages.UserNotFound);
                }

                await _userManager.UpdateSecurityStampAsync(user);

                await _signInManager.SignOutAsync();

                _logger.LogInformation($"User logged out from all devices: {user.Email}");

                return ServiceResult.SuccessResult("Bütün cihazlardan uğurla çıxış etdiniz");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during global logout for user: {userId}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        #endregion

        #region JWT Token Management

        public async Task<ServiceResult<TokenRefreshResponseDto>> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            try
            {
                // Validate refresh token
                if (!_jwtService.ValidateRefreshToken(refreshTokenDto.RefreshToken))
                {
                    return ServiceResult<TokenRefreshResponseDto>.ErrorResult("Refresh token keçərsizdir");
                }

                // Get user from refresh token (implementation needed in database)
                // For now, get user from access token
                var userIdFromToken = _jwtService.GetUserIdFromToken(refreshTokenDto.AccessToken);
                if (string.IsNullOrEmpty(userIdFromToken))
                {
                    return ServiceResult<TokenRefreshResponseDto>.ErrorResult("Token keçərsizdir");
                }

                var user = await _userManager.FindByIdAsync(userIdFromToken);
                if (user == null)
                {
                    return ServiceResult<TokenRefreshResponseDto>.ErrorResult(ErrorMessages.UserNotFound);
                }

                // Generate new tokens
                var newAccessToken = _jwtService.GenerateAccessToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                var response = new TokenRefreshResponseDto
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    Expires = DateTime.Now.AddHours(24)
                };

                _logger.LogInformation($"Token refreshed for user: {user.Email}");
                return ServiceResult<TokenRefreshResponseDto>.SuccessResult(response, "Token yeniləndi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return ServiceResult<TokenRefreshResponseDto>.ErrorResult("Token yenilənmədi");
            }
        }

        public async Task<ServiceResult<User>> ValidateTokenAndGetUserAsync(string token)
        {
            try
            {
                var principal = _jwtService.ValidateToken(token);
                if (principal == null)
                {
                    return ServiceResult<User>.ErrorResult("Token keçərsizdir");
                }

                var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return ServiceResult<User>.ErrorResult("Token-də user ID tapılmadı");
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<User>.ErrorResult(ErrorMessages.UserNotFound);
                }

                return ServiceResult<User>.SuccessResult(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return ServiceResult<User>.ErrorResult("Token validation xətası");
            }
        }

        #endregion

        #region Email Confirmation

        public async Task<ServiceResult> SendEmailConfirmationCodeAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return ServiceResult.ErrorResult(ErrorMessages.UserNotFound);
                }

                if (user.EmailConfirmed)
                {
                    return ServiceResult.ErrorResult("Email artıq təsdiqlənib");
                }

                // 6 rəqəmli confirmation code generate et
                var confirmationCode = CodeGeneratorHelper.GenerateEmailConfirmationCode();
                var expiry = DateTime.Now.AddHours(24); // 24 saat

                // Temporary storage (Production-da Redis)
                _confirmationCodes.AddOrUpdate(email, (confirmationCode, expiry), (key, oldValue) => (confirmationCode, expiry));

                // Email göndər
                var emailSent = await _emailService.SendConfirmationCodeAsync(email, user.FirstName, confirmationCode);

                if (!emailSent)
                {
                    return ServiceResult.ErrorResult("Email göndərilmədi. Yenidən cəhd edin.");
                }

                _logger.LogInformation($"Email confirmation code sent to: {email}");
                return ServiceResult.SuccessResult("Təsdiq kodu email ünvanınıza göndərildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending confirmation code to: {email}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        public async Task<ServiceResult> ConfirmEmailAsync(ConfirmEmailDto confirmEmailDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(confirmEmailDto.Email);
                if (user == null)
                {
                    return ServiceResult.ErrorResult(ErrorMessages.UserNotFound);
                }

                if (user.EmailConfirmed)
                {
                    return ServiceResult.ErrorResult("Email artıq təsdiqlənib");
                }

                // Code yoxlaması
                if (!_confirmationCodes.TryGetValue(confirmEmailDto.Email, out var storedCodeInfo))
                {
                    return ServiceResult.ErrorResult("Təsdiq kodu tapılmadı və ya müddəti bitib");
                }

                if (DateTime.Now > storedCodeInfo.Expiry)
                {
                    _confirmationCodes.TryRemove(confirmEmailDto.Email, out _);
                    return ServiceResult.ErrorResult("Təsdiq kodunun müddəti bitib");
                }

                if (storedCodeInfo.Code != confirmEmailDto.ConfirmationCode)
                {
                    return ServiceResult.ErrorResult("Təsdiq kodu yanlışdır");
                }

                // Email təsdiq et
                user.EmailConfirmed = true;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    // Code-u sil
                    _confirmationCodes.TryRemove(confirmEmailDto.Email, out _);

                    // Welcome email göndər
                    await _emailService.SendWelcomeEmailAsync(user.Email, user.FirstName);

                    _logger.LogInformation($"Email confirmed successfully: {confirmEmailDto.Email}");
                    return ServiceResult.SuccessResult(SuccessMessages.EmailConfirmed);
                }

                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming email: {confirmEmailDto.Email}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        #endregion

        #region Password Reset

        public async Task<ServiceResult> SendPasswordResetCodeAsync(ForgotPasswordDto forgotPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(forgotPasswordDto.Email);
                if (user == null)
                {
                    // Security səbəbindən user-in mövcud olmadığını bildirmirik
                    return ServiceResult.SuccessResult("Əgər bu email mövcuddursa, şifrə sıfırlama kodu göndərildi");
                }

                // 4 rəqəmli reset code generate et
                var resetCode = CodeGeneratorHelper.GeneratePasswordResetCode();
                var expiry = DateTime.Now.AddMinutes(30); // 30 dəqiqə

                // Temporary storage
                _resetCodes.AddOrUpdate(forgotPasswordDto.Email, (resetCode, expiry), (key, oldValue) => (resetCode, expiry));

                // Email göndər
                var emailSent = await _emailService.SendPasswordResetCodeAsync(forgotPasswordDto.Email, user.FirstName, resetCode);

                if (!emailSent)
                {
                    return ServiceResult.ErrorResult("Email göndərilmədi. Yenidən cəhd edin.");
                }

                _logger.LogInformation($"Password reset code sent to: {forgotPasswordDto.Email}");
                return ServiceResult.SuccessResult("Şifrə sıfırlama kodu email ünvanınıza göndərildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending password reset code to: {forgotPasswordDto.Email}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(resetPasswordDto.Email);
                if (user == null)
                {
                    return ServiceResult.ErrorResult(ErrorMessages.UserNotFound);
                }

                // Code yoxlaması
                if (!_resetCodes.TryGetValue(resetPasswordDto.Email, out var storedCodeInfo))
                {
                    return ServiceResult.ErrorResult("Sıfırlama kodu tapılmadı və ya müddəti bitib");
                }

                if (DateTime.Now > storedCodeInfo.Expiry)
                {
                    _resetCodes.TryRemove(resetPasswordDto.Email, out _);
                    return ServiceResult.ErrorResult("Sıfırlama kodunun müddəti bitib");
                }

                if (storedCodeInfo.Code != resetPasswordDto.ResetCode)
                {
                    return ServiceResult.ErrorResult("Sıfırlama kodu yanlışdır");
                }

                // Şifrə sıfırla
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    // Code-u sil
                    _resetCodes.TryRemove(resetPasswordDto.Email, out _);

                    _logger.LogInformation($"Password reset successfully: {resetPasswordDto.Email}");
                    return ServiceResult.SuccessResult(SuccessMessages.PasswordChanged);
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password: {resetPasswordDto.Email}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        #endregion

        #region Password Management

        public async Task<ServiceResult> ChangePasswordAsync(string userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult.ErrorResult(ErrorMessages.UserNotFound);
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"Password changed successfully: {user.Email}");
                    return ServiceResult.SuccessResult(SuccessMessages.PasswordChanged);
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                return ServiceResult.ErrorResult("Şifrə dəyişdirilmədi", errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user: {userId}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        #endregion

        #region Profile Management

        public async Task<ServiceResult<UserResponseDto>> GetUserProfileAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<UserResponseDto>.ErrorResult(ErrorMessages.UserNotFound);
                }

                var userResponse = _mapper.Map<UserResponseDto>(user);
                return ServiceResult<UserResponseDto>.SuccessResult(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user profile: {userId}");
                return ServiceResult<UserResponseDto>.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        public async Task<ServiceResult<UserResponseDto>> UpdateUserProfileAsync(string userId, UpdateProfileDto updateProfileDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult<UserResponseDto>.ErrorResult(ErrorMessages.UserNotFound);
                }

                // Update user properties
                user.FirstName = updateProfileDto.FirstName;
                user.LastName = updateProfileDto.LastName;
                user.PhoneNumber = updateProfileDto.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var userResponse = _mapper.Map<UserResponseDto>(user);
                    _logger.LogInformation($"User profile updated: {user.Email}");
                    return ServiceResult<UserResponseDto>.SuccessResult(userResponse, SuccessMessages.UserUpdated);
                }

                var errors = result.Errors.Select(e => e.Description).ToList();
                return ServiceResult<UserResponseDto>.ErrorResult(ErrorMessages.SomethingWentWrong, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user profile: {userId}");
                return ServiceResult<UserResponseDto>.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        public async Task<ServiceResult<UserResponseDto>> GetUserByEmailAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return ServiceResult<UserResponseDto>.ErrorResult(ErrorMessages.UserNotFound);
                }

                var userResponse = _mapper.Map<UserResponseDto>(user);
                return ServiceResult<UserResponseDto>.SuccessResult(userResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting user by email: {email}");
                return ServiceResult<UserResponseDto>.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        public async Task<ServiceResult<List<UserResponseDto>>> GetAllUsersAsync()
        {
            try
            {
                var users = _userManager.Users
                    .Where(u => !u.IsDeleted) 
                    .ToList();

                var userDtos = _mapper.Map<List<UserResponseDto>>(users);

                return ServiceResult<List<UserResponseDto>>.SuccessResult(userDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return ServiceResult<List<UserResponseDto>>.ErrorResult("İstifadəçilər əldə edilə bilmədi");
            }
        }


        #endregion

        #region Account Management

        public async Task<ServiceResult> ToggleUserStatusAsync(string userId, bool isActive)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult.ErrorResult(ErrorMessages.UserNotFound);
                }

                user.Status = isActive ? UserStatus.Active : UserStatus.Inactive;
                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    var message = isActive ? "Hesab aktivləşdirildi" : "Hesab deaktivləşdirildi";
                    _logger.LogInformation($"User status changed: {user.Email} - {message}");
                    return ServiceResult.SuccessResult(message);
                }

                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error toggling user status: {userId}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        public async Task<ServiceResult> DeleteUserAccountAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return ServiceResult.ErrorResult(ErrorMessages.UserNotFound);
                }

                // Soft delete
                user.IsDeleted = true;
                user.UpdatedDate = DateTime.Now;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    _logger.LogInformation($"User account deleted: {user.Email}");
                    return ServiceResult.SuccessResult(SuccessMessages.UserDeleted);
                }

                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting user account: {userId}");
                return ServiceResult.ErrorResult(ErrorMessages.SomethingWentWrong);
            }
        }

        #endregion

        #region Validation Helpers

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                return user != null;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsUserExistsAsync(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                return user != null;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}