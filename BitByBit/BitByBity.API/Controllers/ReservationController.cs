using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.Reservation;
using BitByBit.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BitByBit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly ILogger<ReservationController> _logger;

        public ReservationController(IReservationService reservationService, ILogger<ReservationController> logger)
        {
            _reservationService = reservationService;
            _logger = logger;
        }

        /// <summary>
        /// Bütün rezervasiyaları əldə etmə (pagination ilə) - Admin üçün
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReservations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _reservationService.GetAllReservationsAsync(page, pageSize);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reservations");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// ID ilə rezervasiya məlumatlarını əldə etmə
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReservationById(int id)
        {
            try
            {
                var result = await _reservationService.GetReservationByIdAsync(id);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return NotFound(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservation by id: {ReservationId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Yeni rezervasiya yaratma
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationCreateDto createDto)
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

                var result = await _reservationService.CreateReservationAsync(userId, createDto);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetReservationById), new { id = result.Data.Id },
                        ApiResponse.SuccessResponse(data: result.Data, message: result.Message));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Rezervasiya məlumatlarını yeniləmə
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationUpdateDto updateDto)
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

                var result = await _reservationService.UpdateReservationAsync(id, userId, updateDto);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation: {ReservationId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Rezervasiya ləğv etmə
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelReservation(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _reservationService.CancelReservationAsync(id, userId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { ReservationId = id },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation: {ReservationId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// İstifadəçinin rezervasiyalarını əldə etmə
        /// </summary>
        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetMyReservations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _reservationService.GetUserReservationsAsync(userId, page, pageSize);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reservations");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// İstifadəçinin aktiv rezervasiyalarını əldə etmə
        /// </summary>
        [HttpGet("my-reservations/active")]
        public async Task<IActionResult> GetMyActiveReservations()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _reservationService.GetUserActiveReservationsAsync(userId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user active reservations");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// İstifadəçinin keçmiş rezervasiyalarını əldə etmə
        /// </summary>
        [HttpGet("my-reservations/past")]
        public async Task<IActionResult> GetMyPastReservations()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _reservationService.GetUserPastReservationsAsync(userId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user past reservations");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// İstifadəçinin gələcək rezervasiyalarını əldə etmə
        /// </summary>
        [HttpGet("my-reservations/upcoming")]
        public async Task<IActionResult> GetMyUpcomingReservations()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _reservationService.GetUserUpcomingReservationsAsync(userId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user upcoming reservations");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// İstifadəçinin rezervasiya xülasəsi
        /// </summary>
        [HttpGet("my-reservations/summary")]
        public async Task<IActionResult> GetMyReservationSummary()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(ApiResponse.ErrorResponse("İstifadəçi müəyyən edilmədi"));
                }

                var result = await _reservationService.GetUserReservationSummaryAsync(userId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reservation summary");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otağın mövcudluğunu yoxlama
        /// </summary>
        [HttpPost("check-availability")]
        public async Task<IActionResult> CheckAvailability([FromBody] AvailabilityCheckDto availabilityDto)
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

                var result = await _reservationService.CheckAvailabilityAsync(availabilityDto);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Boş otaq ID-lərini əldə etmə
        /// </summary>
        [HttpGet("available-rooms")]
        public async Task<IActionResult> GetAvailableRoomIds([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _reservationService.GetAvailableRoomIdsAsync(startDate, endDate);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available room IDs");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otağın rezervasiyalarını əldə etmə
        /// </summary>
        [HttpGet("room/{roomId}")]
        public async Task<IActionResult> GetRoomReservations(int roomId)
        {
            try
            {
                var result = await _reservationService.GetRoomReservationsAsync(roomId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room reservations: {RoomId}", roomId);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Rezervasiya axtarışı
        /// </summary>
        [HttpPost("search")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SearchReservations([FromBody] ReservationSearchDto searchDto)
        {
            try
            {
                var result = await _reservationService.SearchReservationsAsync(searchDto);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservations");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Bu günkü check-in rezervasiyaları
        /// </summary>
        [HttpGet("today/check-ins")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTodayCheckIns()
        {
            try
            {
                var result = await _reservationService.GetTodayCheckInsAsync();

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today check-ins");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Bu günkü check-out rezervasiyaları
        /// </summary>
        [HttpGet("today/check-outs")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetTodayCheckOuts()
        {
            try
            {
                var result = await _reservationService.GetTodayCheckOutsAsync();

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today check-outs");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Rezervasiya statistikaları
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetReservationStatistics()
        {
            try
            {
                var result = await _reservationService.GetReservationStatisticsAsync();

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: result.Data,
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservation statistics");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// JWT Token-dən current user ID-ni əldə etmə
        /// </summary>
        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        #endregion
    }
}