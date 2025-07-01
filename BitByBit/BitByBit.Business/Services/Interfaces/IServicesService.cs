using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.Services;
using BitByBit.Entities.Enums;

namespace BitByBit.Business.Services.Interfaces
{
    public interface IServicesService
    {
        Task<ServiceResult<(IEnumerable<ServicesResponseDto> Services, int TotalCount)>> GetAllServicesAsync(int page = 1, int pageSize = 10);
        Task<ServiceResult<ServicesResponseDto>> GetServiceByIdAsync(int id);
        Task<ServiceResult<ServicesResponseDto>> CreateServiceAsync(ServicesCreateDto createDto);
        Task<ServiceResult<ServicesResponseDto>> UpdateServiceAsync(int id, ServicesUpdateDto updateDto);
        Task<ServiceResult> DeleteServiceAsync(int id);
        Task<ServiceResult<(IEnumerable<ServicesResponseDto> Services, int TotalCount)>> SearchServicesAsync(ServicesSearchDto searchDto);
        Task<ServiceResult<IEnumerable<ServicesResponseDto>>> GetServicesByRoomTypeAsync(RoomType roomType);
        Task<ServiceResult<IEnumerable<ServicesResponseDto>>> SearchServicesByNameAsync(string searchTerm);
        Task<ServiceResult<IEnumerable<ServicesResponseDto>>> GetServicesWithIconsAsync();
        Task<ServiceResult<Dictionary<RoomType, IEnumerable<ServicesResponseDto>>>> GetServicesGroupedByRoomTypeAsync();
        Task<ServiceResult<IEnumerable<ServicesResponseDto>>> GetLatestServicesAsync(int count = 10);
        Task<ServiceResult<IEnumerable<ServicesResponseDto>>> GetOldestServicesAsync(int count = 10);
        Task<ServiceResult<object>> GetServiceStatisticsAsync();
        Task<ServiceResult<Dictionary<RoomType, int>>> GetServiceCountByRoomTypeAsync();
        Task<ServiceResult<object>> GetIconStatisticsAsync();
        Task<ServiceResult<bool>> IsServiceNameUniqueAsync(string serviceName, int? excludeId = null);
        Task<ServiceResult<bool>> ServiceExistsAsync(int serviceId);
        Task<ServiceResult<bool>> ValidateIconUrlAsync(string iconUrl);
        Task<ServiceResult<IEnumerable<ServicesResponseDto>>> CreateMultipleServicesAsync(IEnumerable<ServicesCreateDto> createDtos);
        Task<ServiceResult> DeleteServicesByRoomTypeAsync(RoomType roomType);
    }
}