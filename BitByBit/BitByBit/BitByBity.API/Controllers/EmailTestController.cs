using BitByBit.Business.DTOs.Common;
using BitByBit.Business.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BitByBit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailTestController> _logger;

        public EmailTestController(IEmailService emailService, ILogger<EmailTestController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Test HTML Confirmation Email
        /// </summary>
        [HttpPost("test-confirmation")]
        public async Task<IActionResult> TestConfirmationEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var result = await _emailService.SendConfirmationCodeAsync(
                    request.Email,
                    request.FirstName ?? "Test User",
                    "123456"
                );

                if (result)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { Email = request.Email, Type = "Confirmation" },
                        message: "HTML confirmation email göndərildi"
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse("Email göndərilmədi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test confirmation email");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Test HTML Welcome Email
        /// </summary>
        [HttpPost("test-welcome")]
        public async Task<IActionResult> TestWelcomeEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var result = await _emailService.SendWelcomeEmailAsync(
                    request.Email,
                    request.FirstName ?? "Test User"
                );

                if (result)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { Email = request.Email, Type = "Welcome" },
                        message: "HTML welcome email göndərildi"
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse("Email göndərilmədi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test welcome email");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Test HTML Password Reset Email
        /// </summary>
        [HttpPost("test-reset")]
        public async Task<IActionResult> TestPasswordResetEmail([FromBody] TestEmailRequest request)
        {
            try
            {
                var result = await _emailService.SendPasswordResetCodeAsync(
                    request.Email,
                    request.FirstName ?? "Test User",
                    "4567"
                );

                if (result)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { Email = request.Email, Type = "Password Reset" },
                        message: "HTML password reset email göndərildi"
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse("Email göndərilmədi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test password reset email");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Test Custom HTML Email
        /// </summary>
        [HttpPost("test-custom")]
        public async Task<IActionResult> TestCustomEmail([FromBody] TestCustomEmailRequest request)
        {
            try
            {
                var result = await _emailService.SendCustomHtmlEmailAsync(
                    request.Email,
                    request.Title ?? "Test Email",
                    request.Content ?? "<h2>Bu test email-dir!</h2><p>HTML template işləyir! 🎉</p>",
                    request.PrimaryColor ?? "#667eea"
                );

                if (result)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new
                        {
                            Email = request.Email,
                            Type = "Custom HTML",
                            Title = request.Title,
                            PrimaryColor = request.PrimaryColor
                        },
                       message: "Custom HTML email göndərildi"
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse("Email göndərilmədi"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test custom email");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Test All Email Types at once
        /// </summary>
        [HttpPost("test-all")]
        public async Task<IActionResult> TestAllEmails([FromBody] TestEmailRequest request)
        {
            try
            {
                var firstName = request.FirstName ?? "Test User";
                var results = new List<object>();

                // Test Confirmation Email
                var confirmationResult = await _emailService.SendConfirmationCodeAsync(request.Email, firstName, "123456");
                results.Add(new { Type = "Confirmation", Success = confirmationResult });

                // Wait 2 seconds between emails
                await Task.Delay(2000);

                // Test Welcome Email
                var welcomeResult = await _emailService.SendWelcomeEmailAsync(request.Email, firstName);
                results.Add(new { Type = "Welcome", Success = welcomeResult });

                // Wait 2 seconds between emails
                await Task.Delay(2000);

                // Test Password Reset Email
                var resetResult = await _emailService.SendPasswordResetCodeAsync(request.Email, firstName, "4567");
                results.Add(new { Type = "Password Reset", Success = resetResult });

                var successCount = results.Count(r => ((dynamic)r).Success);

                return Ok(ApiResponse.SuccessResponse(
                    data: new
                    {
                        Email = request.Email,
                        Results = results,
                        SuccessCount = successCount,
                        TotalCount = results.Count
                    },
                    message: $"{successCount}/{results.Count} email uğurla göndərildi"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending test emails");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }
    }

    public class TestEmailRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
    }

    public class TestCustomEmailRequest : TestEmailRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public string? PrimaryColor { get; set; }
    }
}