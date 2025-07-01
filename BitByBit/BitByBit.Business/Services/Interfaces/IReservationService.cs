using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.Reservation;

namespace BitByBit.Business.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ServiceResult<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)>> GetAllReservationsAsync(int page = 1, int pageSize = 10);
        Task<ServiceResult<ReservationResponseDto>> GetReservationByIdAsync(int id);
        Task<ServiceResult<ReservationResponseDto>> CreateReservationAsync(string userId, ReservationCreateDto createDto);
        Task<ServiceResult<ReservationResponseDto>> UpdateReservationAsync(int id, string userId, ReservationUpdateDto updateDto);
        Task<ServiceResult> CancelReservationAsync(int id, string userId);
        Task<ServiceResult<AvailabilityResponseDto>> CheckAvailabilityAsync(AvailabilityCheckDto availabilityDto);
        Task<ServiceResult<IEnumerable<int>>> GetAvailableRoomIdsAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetRoomReservationsAsync(int roomId);
        Task<ServiceResult<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)>> GetUserReservationsAsync(string userId, int page = 1, int pageSize = 10);
        Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetUserActiveReservationsAsync(string userId);
        Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetUserPastReservationsAsync(string userId);
        Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetUserUpcomingReservationsAsync(string userId);
        Task<ServiceResult<UserReservationSummaryDto>> GetUserReservationSummaryAsync(string userId);
        Task<ServiceResult<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)>> SearchReservationsAsync(ReservationSearchDto searchDto);
        Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetTodayCheckInsAsync();
        Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetTodayCheckOutsAsync();
        Task<ServiceResult<object>> GetReservationStatisticsAsync();
        Task<ServiceResult<object>> GetMonthlyReservationReportAsync(int year, int month);
        Task<ServiceResult<object>> GetMostBookedRoomsAsync(int count = 10);
        Task<ServiceResult<double>> GetAverageStayDurationAsync();
        Task<ServiceResult<double>> GetOccupancyRateAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResult<bool>> ValidateReservationDatesAsync(DateTime startDate, DateTime endDate);
        Task<ServiceResult<bool>> CanUserMakeReservationAsync(string userId);
        Task<ServiceResult<bool>> CanUpdateReservationAsync(int reservationId, string userId);
        Task<ServiceResult<bool>> CanCancelReservationAsync(int reservationId, string userId);
        Task<ServiceResult<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)>> GetAllReservationsForAdminAsync(ReservationSearchDto searchDto);
        Task<ServiceResult> AdminCancelReservationAsync(int reservationId, string adminUserId, string reason);
        Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetConflictingReservationsAsync();
    }
}