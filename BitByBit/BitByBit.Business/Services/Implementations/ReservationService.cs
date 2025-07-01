using AutoMapper;
using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.Reservation;
using BitByBit.Business.Services.Interfaces;
using BitByBit.DataAccess.Repository.Interfaces;
using BitByBit.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BitByBit.Business.Services.Implementations
{
    public class ReservationService : IReservationService
    {
        private readonly IRepository<Reservation> _reservationRepository;
        private readonly IRepository<Room> _roomRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IRepository<Reservation> reservationRepository,
            IRepository<Room> roomRepository,
            IMapper mapper,
            ILogger<ReservationService> logger)
        {
            _reservationRepository = reservationRepository;
            _roomRepository = roomRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResult<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)>> GetAllReservationsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (reservations, totalCount) = await _reservationRepository.GetPagedWithCountAsync(page, pageSize, true);
                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<(IEnumerable<ReservationResponseDto>, int)>.SuccessResult(
                    (reservationDtos, totalCount),
                    "Rezervasiyalar uğurla əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reservations");
                return ServiceResult<(IEnumerable<ReservationResponseDto>, int)>.ErrorResult("Rezervasiyalar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<ReservationResponseDto>> GetReservationByIdAsync(int id)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id, true, "Room", "User");
                if (reservation == null)
                {
                    return ServiceResult<ReservationResponseDto>.ErrorResult("Rezervasiya tapılmadı");
                }

                var reservationDto = _mapper.Map<ReservationResponseDto>(reservation);
                return ServiceResult<ReservationResponseDto>.SuccessResult(reservationDto, "Rezervasiya uğurla əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservation by id: {ReservationId}", id);
                return ServiceResult<ReservationResponseDto>.ErrorResult("Rezervasiya əldə edilmədi");
            }
        }

        public async Task<ServiceResult<ReservationResponseDto>> CreateReservationAsync(string userId, ReservationCreateDto createDto)
        {
            try
            {
                if (!createDto.IsValidDateRange())
                {
                    return ServiceResult<ReservationResponseDto>.ErrorResult("Tarixlər düzgün deyil");
                }

                var roomExists = await _roomRepository.ExistsByIdAsync(createDto.RoomId);
                if (!roomExists)
                {
                    return ServiceResult<ReservationResponseDto>.ErrorResult("Otaq tapılmadı");
                }

                var hasConflict = await _reservationRepository.AnyAsync(r =>
                    r.RoomId == createDto.RoomId &&
                    r.StartDate < createDto.EndDate &&
                    r.EndDate > createDto.StartDate
                );

                if (hasConflict)
                {
                    return ServiceResult<ReservationResponseDto>.ErrorResult("Otaq bu tarixlərdə doludur");
                }

                var reservation = _mapper.Map<Reservation>(createDto);
                reservation.UserId = userId;
                reservation.TotalNights = createDto.CalculateTotalNights();

                var createdReservation = await _reservationRepository.AddAsync(reservation);
                await _reservationRepository.SaveChangesAsync();

                var fullReservation = await _reservationRepository.GetByIdAsync(createdReservation.Id, true, "Room", "User");
                var reservationDto = _mapper.Map<ReservationResponseDto>(fullReservation);

                _logger.LogInformation("Reservation created successfully for user: {UserId}, Room: {RoomId}", userId, createDto.RoomId);
                return ServiceResult<ReservationResponseDto>.SuccessResult(reservationDto, "Rezervasiya uğurla yaradıldı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reservation for user: {UserId}", userId);
                return ServiceResult<ReservationResponseDto>.ErrorResult("Rezervasiya yaradılmadı");
            }
        }

        public async Task<ServiceResult<ReservationResponseDto>> UpdateReservationAsync(int id, string userId, ReservationUpdateDto updateDto)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                {
                    return ServiceResult<ReservationResponseDto>.ErrorResult("Rezervasiya tapılmadı");
                }

                if (reservation.UserId != userId)
                {
                    return ServiceResult<ReservationResponseDto>.ErrorResult("Bu rezervasiya sizə aid deyil");
                }

                if (!updateDto.IsValidDateRange())
                {
                    return ServiceResult<ReservationResponseDto>.ErrorResult("Tarixlər düzgün deyil");
                }

                var hasConflict = await _reservationRepository.AnyAsync(r =>
                    r.RoomId == reservation.RoomId &&
                    r.Id != id &&
                    r.StartDate < updateDto.EndDate &&
                    r.EndDate > updateDto.StartDate
                );

                if (hasConflict)
                {
                    return ServiceResult<ReservationResponseDto>.ErrorResult("Otaq yeni tarixlərdə doludur");
                }

                _mapper.Map(updateDto, reservation);
                reservation.TotalNights = updateDto.CalculateTotalNights();

                _reservationRepository.Update(reservation);
                await _reservationRepository.SaveChangesAsync();

                var reservationDto = _mapper.Map<ReservationResponseDto>(reservation);

                _logger.LogInformation("Reservation updated successfully: {ReservationId}", id);
                return ServiceResult<ReservationResponseDto>.SuccessResult(reservationDto, "Rezervasiya uğurla yeniləndi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reservation: {ReservationId}", id);
                return ServiceResult<ReservationResponseDto>.ErrorResult("Rezervasiya yenilənmədi");
            }
        }

        public async Task<ServiceResult> CancelReservationAsync(int id, string userId)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(id);
                if (reservation == null)
                {
                    return ServiceResult.ErrorResult("Rezervasiya tapılmadı");
                }

                if (reservation.UserId != userId)
                {
                    return ServiceResult.ErrorResult("Bu rezervasiya sizə aid deyil");
                }

                _reservationRepository.SoftDelete(reservation);
                await _reservationRepository.SaveChangesAsync();

                _logger.LogInformation("Reservation cancelled successfully: {ReservationId}", id);
                return ServiceResult.SuccessResult("Rezervasiya uğurla ləğv edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling reservation: {ReservationId}", id);
                return ServiceResult.ErrorResult("Rezervasiya ləğv edilmədi");
            }
        }

        public async Task<ServiceResult<AvailabilityResponseDto>> CheckAvailabilityAsync(AvailabilityCheckDto availabilityDto)
        {
            try
            {
                var roomExists = await _roomRepository.ExistsByIdAsync(availabilityDto.RoomId);
                if (!roomExists)
                {
                    return ServiceResult<AvailabilityResponseDto>.ErrorResult("Otaq tapılmadı");
                }

                var hasConflict = await _reservationRepository.AnyAsync(r =>
                    r.RoomId == availabilityDto.RoomId &&
                    r.StartDate < availabilityDto.EndDate &&
                    r.EndDate > availabilityDto.StartDate
                );

                var response = new AvailabilityResponseDto
                {
                    IsAvailable = !hasConflict,
                    Message = hasConflict ? "Otaq bu tarixlərdə doludur" : "Otaq boşdur"
                };

                return ServiceResult<AvailabilityResponseDto>.SuccessResult(response, "Mövcudluq yoxlanıldı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking availability for room: {RoomId}", availabilityDto.RoomId);
                return ServiceResult<AvailabilityResponseDto>.ErrorResult("Mövcudluq yoxlana bilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<int>>> GetAvailableRoomIdsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var allRoomIds = await _roomRepository.GetDistinctAsync(r => r.Id);
                var bookedRoomIds = await _reservationRepository.GetDistinctAsync(
                    r => r.StartDate < endDate && r.EndDate > startDate,
                    r => r.RoomId
                );

                var availableRoomIds = allRoomIds.Except(bookedRoomIds);

                return ServiceResult<IEnumerable<int>>.SuccessResult(
                    availableRoomIds,
                    "Boş otaq ID-ləri əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available room IDs for dates: {StartDate}-{EndDate}", startDate, endDate);
                return ServiceResult<IEnumerable<int>>.ErrorResult("Boş otaqlar tapıla bilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetRoomReservationsAsync(int roomId)
        {
            try
            {
                var reservations = await _reservationRepository.GetByConditionAsync(
                    r => r.RoomId == roomId,
                    true,
                    "Room", "User"
                );

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<IEnumerable<ReservationResponseDto>>.SuccessResult(
                    reservationDtos,
                    "Otaq rezervasiyaları əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room reservations: {RoomId}", roomId);
                return ServiceResult<IEnumerable<ReservationResponseDto>>.ErrorResult("Otaq rezervasiyaları əldə edilmədi");
            }
        }

        public async Task<ServiceResult<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)>> GetUserReservationsAsync(string userId, int page = 1, int pageSize = 10)
        {
            try
            {
                var (reservations, totalCount) = await _reservationRepository.GetPagedWithCountAsync(
                    r => r.UserId == userId,
                    page,
                    pageSize,
                    true
                );

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<(IEnumerable<ReservationResponseDto>, int)>.SuccessResult(
                    (reservationDtos, totalCount),
                    "İstifadəçi rezervasiyaları əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reservations: {UserId}", userId);
                return ServiceResult<(IEnumerable<ReservationResponseDto>, int)>.ErrorResult("İstifadəçi rezervasiyaları əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetUserActiveReservationsAsync(string userId)
        {
            try
            {
                var reservations = await _reservationRepository.GetByConditionAsync(
                    r => r.UserId == userId && r.EndDate > DateTime.Now,
                    true,
                    "Room", "User"
                );

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<IEnumerable<ReservationResponseDto>>.SuccessResult(
                    reservationDtos,
                    "Aktiv rezervasiyalar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user active reservations: {UserId}", userId);
                return ServiceResult<IEnumerable<ReservationResponseDto>>.ErrorResult("Aktiv rezervasiyalar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetUserPastReservationsAsync(string userId)
        {
            try
            {
                var reservations = await _reservationRepository.GetByConditionAsync(
                    r => r.UserId == userId && r.EndDate <= DateTime.Now,
                    true,
                    "Room", "User"
                );

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<IEnumerable<ReservationResponseDto>>.SuccessResult(
                    reservationDtos,
                    "Keçmiş rezervasiyalar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user past reservations: {UserId}", userId);
                return ServiceResult<IEnumerable<ReservationResponseDto>>.ErrorResult("Keçmiş rezervasiyalar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetUserUpcomingReservationsAsync(string userId)
        {
            try
            {
                var reservations = await _reservationRepository.GetByConditionAsync(
                    r => r.UserId == userId && r.StartDate > DateTime.Now,
                    true,
                    "Room", "User"
                );

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<IEnumerable<ReservationResponseDto>>.SuccessResult(
                    reservationDtos,
                    "Gələcək rezervasiyalar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user upcoming reservations: {UserId}", userId);
                return ServiceResult<IEnumerable<ReservationResponseDto>>.ErrorResult("Gələcək rezervasiyalar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<UserReservationSummaryDto>> GetUserReservationSummaryAsync(string userId)
        {
            try
            {
                var totalReservations = await _reservationRepository.CountAsync(r => r.UserId == userId);
                var activeReservations = await _reservationRepository.CountAsync(r => r.UserId == userId && r.EndDate > DateTime.Now);
                var pastReservations = await _reservationRepository.CountAsync(r => r.UserId == userId && r.EndDate <= DateTime.Now);
                var upcomingReservations = await _reservationRepository.CountAsync(r => r.UserId == userId && r.StartDate > DateTime.Now);

                var summary = new UserReservationSummaryDto
                {
                    TotalReservations = totalReservations,
                    UpcomingReservations = upcomingReservations,
                    CompletedReservations = pastReservations
                };

                return ServiceResult<UserReservationSummaryDto>.SuccessResult(
                    summary,
                    "İstifadəçi rezervasiya xülasəsi əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user reservation summary: {UserId}", userId);
                return ServiceResult<UserReservationSummaryDto>.ErrorResult("Rezervasiya xülasəsi əldə edilmədi");
            }
        }

        public async Task<ServiceResult<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)>> SearchReservationsAsync(ReservationSearchDto searchDto)
        {
            try
            {
                var query = _reservationRepository.GetAll(true, "Room", "User");

                if (!string.IsNullOrEmpty(searchDto.UserId))
                {
                    query = query.Where(r => r.UserId == searchDto.UserId);
                }

                if (searchDto.RoomId.HasValue)
                {
                    query = query.Where(r => r.RoomId == searchDto.RoomId.Value);
                }

                if (searchDto.StartDate.HasValue)
                {
                    query = query.Where(r => r.StartDate >= searchDto.StartDate.Value);
                }

                if (searchDto.EndDate.HasValue)
                {
                    query = query.Where(r => r.EndDate <= searchDto.EndDate.Value);
                }

                var totalCount = await query.CountAsync();
                var reservations = await query.Skip((searchDto.Page - 1) * searchDto.PageSize)
                                             .Take(searchDto.PageSize)
                                             .ToListAsync();

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<(IEnumerable<ReservationResponseDto>, int)>.SuccessResult(
                    (reservationDtos, totalCount),
                    "Axtarış nəticələri əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching reservations");
                return ServiceResult<(IEnumerable<ReservationResponseDto>, int)>.ErrorResult("Axtarış zamanı xəta baş verdi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var reservations = await _reservationRepository.GetByConditionAsync(
                    r => r.StartDate >= startDate && r.StartDate <= endDate,
                    true,
                    "Room", "User"
                );

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<IEnumerable<ReservationResponseDto>>.SuccessResult(
                    reservationDtos,
                    "Tarix aralığındakı rezervasiyalar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservations by date range: {StartDate}-{EndDate}", startDate, endDate);
                return ServiceResult<IEnumerable<ReservationResponseDto>>.ErrorResult("Rezervasiyalar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetTodayCheckInsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var reservations = await _reservationRepository.GetByConditionAsync(
                    r => r.StartDate >= today && r.StartDate < tomorrow,
                    true,
                    "Room", "User"
                );

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<IEnumerable<ReservationResponseDto>>.SuccessResult(
                    reservationDtos,
                    "Bu günkü check-in rezervasiyaları əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today check-ins");
                return ServiceResult<IEnumerable<ReservationResponseDto>>.ErrorResult("Bu günkü check-in rezervasiyaları əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetTodayCheckOutsAsync()
        {
            try
            {
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);

                var reservations = await _reservationRepository.GetByConditionAsync(
                    r => r.EndDate >= today && r.EndDate < tomorrow,
                    true,
                    "Room", "User"
                );

                var reservationDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(reservations);

                return ServiceResult<IEnumerable<ReservationResponseDto>>.SuccessResult(
                    reservationDtos,
                    "Bu günkü check-out rezervasiyaları əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting today check-outs");
                return ServiceResult<IEnumerable<ReservationResponseDto>>.ErrorResult("Bu günkü check-out rezervasiyaları əldə edilmədi");
            }
        }

        public async Task<ServiceResult<object>> GetReservationStatisticsAsync()
        {
            try
            {
                var totalReservations = await _reservationRepository.CountAsync();
                var activeReservations = await _reservationRepository.CountAsync(r => r.EndDate > DateTime.Now);

                var stats = new
                {
                    TotalReservations = totalReservations,
                    ActiveReservations = activeReservations
                };

                return ServiceResult<object>.SuccessResult(stats, "Rezervasiya statistikaları əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reservation statistics");
                return ServiceResult<object>.ErrorResult("Statistikalar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<object>> GetMonthlyReservationReportAsync(int year, int month)
        {
            try
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1);

                var reservations = await _reservationRepository.GetByConditionAsync(
                    r => r.StartDate >= startDate && r.StartDate < endDate
                );

                var totalCount = reservations.Count();
                var totalNights = reservations.Sum(r => r.TotalNights);

                var report = new
                {
                    Year = year,
                    Month = month,
                    TotalReservations = totalCount,
                    TotalNights = totalNights
                };

                return ServiceResult<object>.SuccessResult(report, "Aylıq hesabat əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monthly reservation report: {Year}-{Month}", year, month);
                return ServiceResult<object>.ErrorResult("Aylıq hesabat əldə edilmədi");
            }
        }

        public async Task<ServiceResult<object>> GetMostBookedRoomsAsync(int count = 10)
        {
            try
            {
                var reservations = await _reservationRepository.GetAllAsync(true, "Room");
                var roomBookings = reservations.GroupBy(r => r.RoomId)
                                              .Select(g => new
                                              {
                                                  RoomId = g.Key,
                                                  RoomName = g.First().Room?.RoomName ?? "Unknown",
                                                  BookingCount = g.Count()
                                              })
                                              .OrderByDescending(x => x.BookingCount)
                                              .Take(count);

                return ServiceResult<object>.SuccessResult(roomBookings, "Ən məşhur otaqlar əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most booked rooms");
                return ServiceResult<object>.ErrorResult("Məşhur otaqlar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<double>> GetAverageStayDurationAsync()
        {
            try
            {
                var averageNights = await _reservationRepository.AverageAsync(r => r.TotalNights);
                return ServiceResult<double>.SuccessResult(averageNights, "Ortalama qalma müddəti hesablandı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average stay duration");
                return ServiceResult<double>.ErrorResult("Ortalama müddət hesablana bilmədi");
            }
        }

        public async Task<ServiceResult<double>> GetOccupancyRateAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var totalRooms = await _roomRepository.CountAsync();
                var totalDays = (endDate - startDate).Days;
                var totalRoomDays = totalRooms * totalDays;

                if (totalRoomDays == 0)
                {
                    return ServiceResult<double>.SuccessResult(0, "Doluluk dərəcəsi hesablandı");
                }

                var bookedNights = await _reservationRepository.SumAsync(
                    r => r.StartDate < endDate && r.EndDate > startDate,
                    r => r.TotalNights
                );

                var occupancyRate = (double)bookedNights / totalRoomDays * 100;

                return ServiceResult<double>.SuccessResult(
                    Math.Round(occupancyRate, 2),
                    "Doluluk dərəcəsi hesablandı"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating occupancy rate");
                return ServiceResult<double>.ErrorResult("Doluluk dərəcəsi hesablana bilmədi");
            }
        }

        public async Task<ServiceResult<bool>> ValidateReservationDatesAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                if (startDate >= endDate)
                {
                    return ServiceResult<bool>.SuccessResult(false, "Başlama tarixi bitirmə tarixindən əvvəl olmalıdır");
                }

                if (startDate < DateTime.Today)
                {
                    return ServiceResult<bool>.SuccessResult(false, "Rezervasiya keçmiş tarixə edilə bilməz");
                }

                return ServiceResult<bool>.SuccessResult(true, "Tarixlər düzgündür");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating reservation dates");
                return ServiceResult<bool>.ErrorResult("Tarix yoxlanıla bilmədi");
            }
        }

        public async Task<ServiceResult<bool>> CanUserMakeReservationAsync(string userId)
        {
            try
            {
                var activeReservations = await _reservationRepository.CountAsync(
                    r => r.UserId == userId && r.EndDate > DateTime.Now
                );

                if (activeReservations >= 5)
                {
                    return ServiceResult<bool>.SuccessResult(false, "Çox aktiv rezervasiyanız var");
                }

                return ServiceResult<bool>.SuccessResult(true, "İstifadəçi rezervasiya edə bilər");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user can make reservation: {UserId}", userId);
                return ServiceResult<bool>.ErrorResult("İcazə yoxlana bilmədi");
            }
        }

        public async Task<ServiceResult<bool>> CanUpdateReservationAsync(int reservationId, string userId)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(reservationId);
                if (reservation == null)
                {
                    return ServiceResult<bool>.SuccessResult(false, "Rezervasiya tapılmadı");
                }

                if (reservation.UserId != userId)
                {
                    return ServiceResult<bool>.SuccessResult(false, "Bu rezervasiya sizə aid deyil");
                }

                return ServiceResult<bool>.SuccessResult(true, "Rezervasiya yenilənə bilər");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if reservation can be updated: {ReservationId}", reservationId);
                return ServiceResult<bool>.ErrorResult("Yeniləmə icazəsi yoxlana bilmədi");
            }
        }

        public async Task<ServiceResult<bool>> CanCancelReservationAsync(int reservationId, string userId)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(reservationId);
                if (reservation == null)
                {
                    return ServiceResult<bool>.SuccessResult(false, "Rezervasiya tapılmadı");
                }

                if (reservation.UserId != userId)
                {
                    return ServiceResult<bool>.SuccessResult(false, "Bu rezervasiya sizə aid deyil");
                }

                return ServiceResult<bool>.SuccessResult(true, "Rezervasiya ləğv edilə bilər");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if reservation can be cancelled: {ReservationId}", reservationId);
                return ServiceResult<bool>.ErrorResult("Ləğv icazəsi yoxlana bilmədi");
            }
        }

        public async Task<ServiceResult<(IEnumerable<ReservationResponseDto> Reservations, int TotalCount)>> GetAllReservationsForAdminAsync(ReservationSearchDto searchDto)
        {
            try
            {
                return await SearchReservationsAsync(searchDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all reservations for admin");
                return ServiceResult<(IEnumerable<ReservationResponseDto>, int)>.ErrorResult("Admin rezervasiyaları əldə edilmədi");
            }
        }

        public async Task<ServiceResult> AdminCancelReservationAsync(int reservationId, string adminUserId, string reason)
        {
            try
            {
                var reservation = await _reservationRepository.GetByIdAsync(reservationId);
                if (reservation == null)
                {
                    return ServiceResult.ErrorResult("Rezervasiya tapılmadı");
                }

                _reservationRepository.SoftDelete(reservation);
                await _reservationRepository.SaveChangesAsync();

                _logger.LogInformation("Reservation cancelled by admin: {AdminUserId}, Reservation: {ReservationId}, Reason: {Reason}",
                    adminUserId, reservationId, reason);

                return ServiceResult.SuccessResult("Rezervasiya admin tərəfindən ləğv edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error admin cancelling reservation: {ReservationId}", reservationId);
                return ServiceResult.ErrorResult("Admin rezervasiya ləğvi uğursuz");
            }
        }

        public async Task<ServiceResult<IEnumerable<ReservationResponseDto>>> GetConflictingReservationsAsync()
        {
            try
            {
                var allReservations = await _reservationRepository.GetAllAsync(true, "Room", "User");
                var conflictingReservations = new List<Reservation>();

                foreach (var reservation in allReservations)
                {
                    var conflicts = allReservations.Where(r =>
                        r.Id != reservation.Id &&
                        r.RoomId == reservation.RoomId &&
                        r.StartDate < reservation.EndDate &&
                        r.EndDate > reservation.StartDate
                    );

                    if (conflicts.Any())
                    {
                        conflictingReservations.Add(reservation);
                    }
                }

                var conflictDtos = _mapper.Map<IEnumerable<ReservationResponseDto>>(conflictingReservations.Distinct());

                return ServiceResult<IEnumerable<ReservationResponseDto>>.SuccessResult(
                    conflictDtos,
                    "Qarşıdurma olan rezervasiyalar tapıldı"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting conflicting reservations");
                return ServiceResult<IEnumerable<ReservationResponseDto>>.ErrorResult("Qarşıdurma olan rezervasiyalar tapıla bilmədi");
            }
        }
    }
}