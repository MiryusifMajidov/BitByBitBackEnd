using BitByBit.Business.DTOs.Auth;
using BitByBit.Business.DTOs.Common;
using BitByBit.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BitByBity.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JwtTestController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IUserService _userService;
        private readonly ILogger<JwtTestController> _logger;

        public JwtTestController(IJwtService jwtService, IUserService userService, ILogger<JwtTestController> logger)
        {
            _jwtService = jwtService;
            _userService = userService;
            _logger = logger;
        }

      
        [HttpPost("decode-token")]
        public IActionResult DecodeToken([FromBody] string token)
        {
            try
            {
                var tokenInfo = ((BitByBit.Business.Services.Implementations.JwtService)_jwtService).DecodeToken(token);

                return Ok(ApiResponse.SuccessResponse(
                    data: tokenInfo,
                    message: "Token decode edildi"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decoding token");
                return BadRequest(ApiResponse.ErrorResponse("Token decode edilmədi"));
            }
        }

        
        [HttpPost("validate-token")]
        public IActionResult ValidateToken([FromBody] string token)
        {
            try
            {
                var principal = _jwtService.ValidateToken(token);

                if (principal != null)
                {
                    var claims = principal.Claims.Select(c => new { c.Type, c.Value }).ToList();

                    return Ok(ApiResponse.SuccessResponse(
                        data: new
                        {
                            IsValid = true,
                            UserId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                            Email = principal.FindFirst(ClaimTypes.Email)?.Value,
                            Role = principal.FindFirst(ClaimTypes.Role)?.Value,
                            Claims = claims
                        },
                        message: "Token keçərlidir"
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse("Token keçərsizdir"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return BadRequest(ApiResponse.ErrorResponse("Token validation xətası"));
            }
        }

     
        [HttpGet("my-token-info")]
        [Authorize]
        public IActionResult GetMyTokenInfo()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                var role = User.FindFirst(ClaimTypes.Role)?.Value;
                var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value;
                var lastName = User.FindFirst(ClaimTypes.Surname)?.Value;

                var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();

                return Ok(ApiResponse.SuccessResponse(
                    data: new TokenInfoDto
                    {
                        UserId = userId,
                        Email = email,
                        Role = role,
                        FirstName = firstName,
                        LastName = lastName,
                        Claims = claims.Select(c => new ClaimInfo { Type = c.Type, Value = c.Value }).ToList()
                    },
                    message: "Token məlumatları"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting token info");
                return StatusCode(500, ApiResponse.ErrorResponse("Token məlumatları alınmadı"));
            }
        }

        /// <summary>
        /// Test endpoint - yalnız Admin-lər üçün
        /// </summary>
        [HttpGet("admin-only")]
        [Authorize(Roles = "Admin")]
        public IActionResult AdminOnlyEndpoint()
        {
            var userName = User.Identity?.Name;
            return Ok(ApiResponse.SuccessResponse(
                data: new { Message = $"Salam Admin {userName}! Bu endpoint yalnız adminlər üçündür." },
                message: "Admin access təsdiqləndi"
            ));
        }

        /// <summary>
        /// Test endpoint - yalnız User-lər üçün
        /// </summary>
        [HttpGet("user-only")]
        [Authorize(Roles = "User")]
        public IActionResult UserOnlyEndpoint()
        {
            var userName = User.Identity?.Name;
            return Ok(ApiResponse.SuccessResponse(
                data: new { Message = $"Salam {userName}! Bu endpoint yalnız istifadəçilər üçündür." },
                message: "User access təsdiqləndi"
            ));
        }

        /// <summary>
        /// Test endpoint - hər hansı authenticated user üçün
        /// </summary>
        [HttpGet("authenticated-only")]
        [Authorize]
        public IActionResult AuthenticatedOnlyEndpoint()
        {
            var userName = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            return Ok(ApiResponse.SuccessResponse(
                data: new
                {
                    Message = $"Salam {userName}! Sən {role} kimi daxil olmusan.",
                    AuthenticatedAt = DateTime.Now,
                    UserInfo = new
                    {
                        Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                        Email = User.FindFirst(ClaimTypes.Email)?.Value,
                        Role = role
                    }
                },
                message: "Authentication təsdiqləndi"
            ));
        }

        /// <summary>
        /// Generate custom test token
        /// </summary>
        [HttpPost("generate-test-token")]
        public IActionResult GenerateTestToken([FromBody] TestTokenRequest request)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, request.UserId ?? "test-user-123"),
                    new(ClaimTypes.Email, request.Email ?? "test@bitbybit.com"),
                    new(ClaimTypes.Role, request.Role ?? "User"),
                    new(ClaimTypes.GivenName, request.FirstName ?? "Test"),
                    new(ClaimTypes.Surname, request.LastName ?? "User")
                };

                var token = _jwtService.GenerateToken(claims);

                return Ok(ApiResponse.SuccessResponse(
                    data: new
                    {
                        Token = token,
                        Expires = DateTime.Now.AddHours(24),
                        Claims = claims.Select(c => new { c.Type, c.Value })
                    },
                    message: "Test token yaradıldı"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating test token");
                return StatusCode(500, ApiResponse.ErrorResponse("Test token yaradılmadı"));
            }
        }
    }

    public class TestTokenRequest
    {
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}