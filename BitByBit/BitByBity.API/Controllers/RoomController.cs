using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.Room;
using BitByBit.Business.Services.Interfaces;
using BitByBit.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BitByBit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;
        private readonly ILogger<RoomController> _logger;

        public RoomController(IRoomService roomService, ILogger<RoomController> logger)
        {
            _roomService = roomService;
            _logger = logger;
        }

        /// <summary>
        /// Bütün otaqları əldə etmə (pagination ilə)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllRooms([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _roomService.GetAllRoomsAsync(page, pageSize);

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
                _logger.LogError(ex, "Error getting all rooms");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// ID ilə otaq məlumatlarını əldə etmə
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoomById(int id)
        {
            try
            {
                var result = await _roomService.GetRoomByIdAsync(id);

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
                _logger.LogError(ex, "Error getting room by id: {RoomId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Yeni otaq yaratma
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRoom([FromBody] RoomCreateDto createDto)
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

                var result = await _roomService.CreateRoomAsync(createDto);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetRoomById), new { id = result.Data.Id },
                        ApiResponse.SuccessResponse(data: result.Data, message: result.Message));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room: {RoomName}", createDto.RoomName);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otaq məlumatlarını yeniləmə
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRoom(int id, [FromBody] RoomUpdateDto updateDto)
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

                var result = await _roomService.UpdateRoomAsync(id, updateDto);

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
                _logger.LogError(ex, "Error updating room: {RoomId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otaq silmə
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            try
            {
                var result = await _roomService.DeleteRoomAsync(id);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { RoomId = id },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room: {RoomId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otaq axtarışı və filtrlənməsi
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchRooms([FromBody] RoomSearchDto searchDto)
        {
            try
            {
                var result = await _roomService.SearchRoomsAsync(searchDto);

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
                _logger.LogError(ex, "Error searching rooms");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otaq tipinə görə otaqları əldə etmə
        /// </summary>
        [HttpGet("by-type/{roomType}")]
        public async Task<IActionResult> GetRoomsByType(RoomType roomType)
        {
            try
            {
                var result = await _roomService.GetRoomsByTypeAsync(roomType);

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
                _logger.LogError(ex, "Error getting rooms by type: {RoomType}", roomType);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Qiymət aralığına görə otaqları əldə etmə
        /// </summary>
        [HttpGet("by-price")]
        public async Task<IActionResult> GetRoomsByPriceRange([FromQuery] decimal minPrice, [FromQuery] decimal maxPrice)
        {
            try
            {
                var result = await _roomService.GetRoomsByPriceRangeAsync(minPrice, maxPrice);

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
                _logger.LogError(ex, "Error getting rooms by price range: {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// WiFi olan otaqları əldə etmə
        /// </summary>
        [HttpGet("wifi-enabled")]
        public async Task<IActionResult> GetWifiEnabledRooms()
        {
            try
            {
                var result = await _roomService.GetWifiEnabledRoomsAsync();

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
                _logger.LogError(ex, "Error getting wifi enabled rooms");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Boş otaqları əldə etmə
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableRooms([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            try
            {
                var result = await _roomService.GetAvailableRoomsAsync(startDate, endDate);

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
                _logger.LogError(ex, "Error getting available rooms");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otağa şəkil əlavə etmə
        /// </summary>
        [HttpPost("{roomId}/images")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddRoomImage(int roomId, [FromBody] ImageCreateDto imageDto)
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

                imageDto.RoomId = roomId;
                var result = await _roomService.AddRoomImageAsync(imageDto);

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
                _logger.LogError(ex, "Error adding room image for room: {RoomId}", roomId);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otaq şəklini silmə
        /// </summary>
        [HttpDelete("images/{imageId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteRoomImage(int imageId)
        {
            try
            {
                var result = await _roomService.DeleteRoomImageAsync(imageId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { ImageId = imageId },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room image: {ImageId}", imageId);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otaq şəkillərini əldə etmə
        /// </summary>
        [HttpGet("{roomId}/images")]
        public async Task<IActionResult> GetRoomImages(int roomId)
        {
            try
            {
                var result = await _roomService.GetRoomImagesAsync(roomId);

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
                _logger.LogError(ex, "Error getting room images for room: {RoomId}", roomId);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Əsas şəkil təyin etmə
        /// </summary>
        [HttpPatch("{roomId}/images/{imageId}/set-main")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SetMainImage(int roomId, int imageId)
        {
            try
            {
                var result = await _roomService.SetMainImageAsync(roomId, imageId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { RoomId = roomId, ImageId = imageId },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting main image: {ImageId} for room: {RoomId}", imageId, roomId);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }
        /// <summary>
        /// Tarix aralığı və tip üzrə boş otaqları axtarış
        /// </summary>
        [HttpGet("search-available")]
        public async Task<IActionResult> SearchAvailableRooms(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] RoomType? roomType = null)
        {
            try
            {
                // Input validation
                if (startDate >= endDate)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Başlama tarixi bitirmə tarixindən əvvəl olmalıdır"));
                }

                if (startDate < DateTime.Today)
                {
                    return BadRequest(ApiResponse.ErrorResponse("Başlama tarixi bu günədək və ya keçmiş ola bilməz"));
                }

                var result = await _roomService.SearchAvailableRoomsByTypeAsync(startDate, endDate, roomType);

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
                _logger.LogError(ex, "Error searching available rooms for dates: {StartDate}-{EndDate}, Type: {RoomType}",
                    startDate, endDate, roomType);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }
        /// <summary>
        /// Otaq statistikaları
        /// </summary>
        [HttpGet("{roomId}/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetRoomStatistics(int roomId)
        {
            try
            {
                var result = await _roomService.GetRoomStatisticsAsync(roomId);

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
                _logger.LogError(ex, "Error getting room statistics: {RoomId}", roomId);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }
    }
}