using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.Services;
using BitByBit.Business.Services.Interfaces;
using BitByBit.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BitByBit.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServicesService _servicesService;
        private readonly ILogger<ServicesController> _logger;

        public ServicesController(IServicesService servicesService, ILogger<ServicesController> logger)
        {
            _servicesService = servicesService;
            _logger = logger;
        }

        /// <summary>
        /// Bütün xidmətləri əldə etmə (pagination ilə)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllServices([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _servicesService.GetAllServicesAsync(page, pageSize);

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
                _logger.LogError(ex, "Error getting all services");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// ID ilə xidmət məlumatlarını əldə etmə
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(int id)
        {
            try
            {
                var result = await _servicesService.GetServiceByIdAsync(id);

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
                _logger.LogError(ex, "Error getting service by id: {ServiceId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Yeni xidmət yaratma
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateService([FromBody] ServicesCreateDto createDto)
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

                var result = await _servicesService.CreateServiceAsync(createDto);

                if (result.Success)
                {
                    return CreatedAtAction(nameof(GetServiceById), new { id = result.Data.Id },
                        ApiResponse.SuccessResponse(data: result.Data, message: result.Message));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service: {ServiceName}", createDto.ServiceName);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Xidmət məlumatlarını yeniləmə
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] ServicesUpdateDto updateDto)
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

                var result = await _servicesService.UpdateServiceAsync(id, updateDto);

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
                _logger.LogError(ex, "Error updating service: {ServiceId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Xidmət silmə
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            try
            {
                var result = await _servicesService.DeleteServiceAsync(id);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { ServiceId = id },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service: {ServiceId}", id);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Xidmət axtarışı və filtrlənməsi
        /// </summary>
        [HttpPost("search")]
        public async Task<IActionResult> SearchServices([FromBody] ServicesSearchDto searchDto)
        {
            try
            {
                var result = await _servicesService.SearchServicesAsync(searchDto);

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
                _logger.LogError(ex, "Error searching services");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otaq tipinə görə xidmətləri əldə etmə
        /// </summary>
        [HttpGet("by-room-type/{roomType}")]
        public async Task<IActionResult> GetServicesByRoomType(RoomType roomType)
        {
            try
            {
                var result = await _servicesService.GetServicesByRoomTypeAsync(roomType);

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
                _logger.LogError(ex, "Error getting services by room type: {RoomType}", roomType);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Ad ilə xidmət axtarışı
        /// </summary>
        [HttpGet("search-by-name")]
        public async Task<IActionResult> SearchServicesByName([FromQuery] string searchTerm)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Axtarış termini tələb olunur"));
                }

                var result = await _servicesService.SearchServicesByNameAsync(searchTerm);

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
                _logger.LogError(ex, "Error searching services by name: {SearchTerm}", searchTerm);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// İkon olan xidmətləri əldə etmə
        /// </summary>
        [HttpGet("with-icons")]
        public async Task<IActionResult> GetServicesWithIcons()
        {
            try
            {
                var result = await _servicesService.GetServicesWithIconsAsync();

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
                _logger.LogError(ex, "Error getting services with icons");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Xidmətləri otaq tipinə görə qruplaşdırmaq
        /// </summary>
        [HttpGet("grouped-by-room-type")]
        public async Task<IActionResult> GetServicesGroupedByRoomType()
        {
            try
            {
                var result = await _servicesService.GetServicesGroupedByRoomTypeAsync();

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
                _logger.LogError(ex, "Error grouping services by room type");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Ən yeni xidmətləri əldə etmə
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestServices([FromQuery] int count = 10)
        {
            try
            {
                var result = await _servicesService.GetLatestServicesAsync(count);

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
                _logger.LogError(ex, "Error getting latest services");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Ən köhnə xidmətləri əldə etmə
        /// </summary>
        [HttpGet("oldest")]
        public async Task<IActionResult> GetOldestServices([FromQuery] int count = 10)
        {
            try
            {
                var result = await _servicesService.GetOldestServicesAsync(count);

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
                _logger.LogError(ex, "Error getting oldest services");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Xidmət statistikaları
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetServiceStatistics()
        {
            try
            {
                var result = await _servicesService.GetServiceStatisticsAsync();

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
                _logger.LogError(ex, "Error getting service statistics");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otaq tipinə görə xidmət sayları
        /// </summary>
        [HttpGet("count-by-room-type")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetServiceCountByRoomType()
        {
            try
            {
                var result = await _servicesService.GetServiceCountByRoomTypeAsync();

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
                _logger.LogError(ex, "Error getting service count by room type");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// İkon statistikaları
        /// </summary>
        [HttpGet("icon-statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetIconStatistics()
        {
            try
            {
                var result = await _servicesService.GetIconStatisticsAsync();

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
                _logger.LogError(ex, "Error getting icon statistics");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Xidmət adının unikal olub-olmadığını yoxlama
        /// </summary>
        [HttpGet("check-name-unique")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> IsServiceNameUnique([FromQuery] string serviceName, [FromQuery] int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrEmpty(serviceName))
                {
                    return BadRequest(ApiResponse.ErrorResponse("Xidmət adı tələb olunur"));
                }

                var result = await _servicesService.IsServiceNameUniqueAsync(serviceName, excludeId);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { IsUnique = result.Data, ServiceName = serviceName },
                        message: result.Data ? "Ad unikaldır" : "Bu ad artıq mövcuddur"
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking service name uniqueness: {ServiceName}", serviceName);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Çoxlu xidmət yaratma
        /// </summary>
        [HttpPost("bulk-create")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateMultipleServices([FromBody] IEnumerable<ServicesCreateDto> createDtos)
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

                var result = await _servicesService.CreateMultipleServicesAsync(createDtos);

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
                _logger.LogError(ex, "Error creating multiple services");
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }

        /// <summary>
        /// Otaq tipinə görə xidmətləri silmə
        /// </summary>
        [HttpDelete("by-room-type/{roomType}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteServicesByRoomType(RoomType roomType)
        {
            try
            {
                var result = await _servicesService.DeleteServicesByRoomTypeAsync(roomType);

                if (result.Success)
                {
                    return Ok(ApiResponse.SuccessResponse(
                        data: new { RoomType = roomType },
                        message: result.Message
                    ));
                }

                return BadRequest(ApiResponse.ErrorResponse(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting services by room type: {RoomType}", roomType);
                return StatusCode(500, ApiResponse.ErrorResponse("Sistemdə xəta baş verdi"));
            }
        }
    }
}