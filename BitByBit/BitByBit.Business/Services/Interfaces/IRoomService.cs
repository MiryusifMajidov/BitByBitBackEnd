using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.Room;
using BitByBit.Entities.Enums;

namespace BitByBit.Business.Services.Interfaces
{
    public interface IRoomService
    {
        // Basic CRUD Operations
        Task<ServiceResult<(IEnumerable<RoomResponseDto> Rooms, int TotalCount)>> GetAllRoomsAsync(int page = 1, int pageSize = 10);
        Task<ServiceResult<RoomResponseDto>> GetRoomByIdAsync(int id);
        Task<ServiceResult<IEnumerable<RoomListResponseDto>>> GetRoomListAsync();

        Task<ServiceResult<RoomResponseDto>> CreateRoomAsync(RoomCreateDto createDto);
        Task<ServiceResult<RoomResponseDto>> UpdateRoomAsync(int id, RoomUpdateDto updateDto);
        Task<ServiceResult> DeleteRoomAsync(int id);

        // Search and Filter Operations
        Task<ServiceResult<(IEnumerable<RoomResponseDto> Rooms, int TotalCount)>> SearchRoomsAsync(RoomSearchDto searchDto);
        Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetRoomsByTypeAsync(RoomType roomType);
        Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetRoomsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetRoomsByCapacityAsync(int minCapacity, int maxCapacity);
        Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetWifiEnabledRoomsAsync();

        // Availability Operations
        Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetAvailableRoomsAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResult<IEnumerable<RoomResponseDto>>> SearchAvailableRoomsByTypeAsync(DateTime startDate, DateTime endDate, RoomType? roomType = null);
        Task<ServiceResult<bool>> IsRoomAvailableAsync(int roomId, DateTime startDate, DateTime endDate);
        Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetMostPopularRoomsAsync(int count = 10);

        // Image Management
        Task<ServiceResult<ImageResponseDto>> AddRoomImageAsync(ImageCreateDto imageDto);
        Task<ServiceResult> DeleteRoomImageAsync(int imageId);
        Task<ServiceResult> SetMainImageAsync(int roomId, int imageId);
        Task<ServiceResult<IEnumerable<ImageResponseDto>>> GetRoomImagesAsync(int roomId);
        
        // Statistics and Analytics
        Task<ServiceResult<object>> GetRoomStatisticsAsync(int roomId);
        Task<ServiceResult<object>> GetOverallRoomStatisticsAsync();
        Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetLeastPopularRoomsAsync(int count = 10);

        // Validation Helpers
        Task<ServiceResult<bool>> IsRoomNameUniqueAsync(string roomName, int? excludeId = null);
        Task<ServiceResult<bool>> RoomExistsAsync(int roomId);
    }
}