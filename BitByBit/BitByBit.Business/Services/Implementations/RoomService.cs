using AutoMapper;
using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.Room;
using BitByBit.Business.Extensions;
using BitByBit.Business.Services.Interfaces;
using BitByBit.DataAccess.Repository.Interfaces;
using BitByBit.Entities.Constants;
using BitByBit.Entities.Enums;
using BitByBit.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BitByBit.Business.Services.Implementations
{
    public class RoomService : IRoomService
    {
        private readonly IRepository<Room> _roomRepository;
        private readonly IRepository<Images> _imageRepository;
        private readonly IRepository<Reservation> _reservationRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<RoomService> _logger;

        public RoomService(
            IRepository<Room> roomRepository,
            IRepository<Images> imageRepository,
            IRepository<Reservation> reservationRepository,
            IMapper mapper,
            ILogger<RoomService> logger)
        {
            _roomRepository = roomRepository;
            _imageRepository = imageRepository;
            _reservationRepository = reservationRepository;
            _mapper = mapper;
            _logger = logger;
        }

        #region Basic CRUD Operations

        public async Task<ServiceResult<(IEnumerable<RoomResponseDto> Rooms, int TotalCount)>> GetAllRoomsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (rooms, totalCount) = await _roomRepository.GetPagedWithCountAsync(page, pageSize, true);
                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);

                return ServiceResult<(IEnumerable<RoomResponseDto>, int)>.SuccessResult(
                    (roomDtos, totalCount),
                    "Otaqlar uğurla əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all rooms");
                return ServiceResult<(IEnumerable<RoomResponseDto>, int)>.ErrorResult("Otaqlar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<RoomResponseDto>> GetRoomByIdAsync(int id)
        {
            try
            {
                var room = await _roomRepository.GetByIdAsync(id, true, "Images");
                if (room == null)
                {
                    return ServiceResult<RoomResponseDto>.ErrorResult("Otaq tapılmadı");
                }

                var roomDto = _mapper.Map<RoomResponseDto>(room);
                return ServiceResult<RoomResponseDto>.SuccessResult(roomDto, "Otaq uğurla əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room by id: {RoomId}", id);
                return ServiceResult<RoomResponseDto>.ErrorResult("Otaq əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<RoomListResponseDto>>> GetRoomListAsync()
        {
            try
            {
                var rooms = await _roomRepository.GetAll(true, "Images").ToListAsync();

                var roomListDtos = rooms.Select(room => new RoomListResponseDto
                {
                    Id = room.Id,
                    RoomName = room.RoomName,
                    Role = room.Role,
                    Price = room.Price,
                    Capacity = room.Capacity,
                    Wifi = room.Wifi,
                    MainImageUrl = room.Images.FirstOrDefault(i => i.IsMain)?.ImageUrl
                }).ToList();

                return ServiceResult<IEnumerable<RoomListResponseDto>>.SuccessResult(
                    roomListDtos,
                    "Otaq siyahısı uğurla əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room list for dropdown or selection");
                return ServiceResult<IEnumerable<RoomListResponseDto>>.ErrorResult("Otaq siyahısı əldə edilmədi");
            }
        }


        public async Task<ServiceResult<RoomResponseDto>> CreateRoomAsync(RoomCreateDto createDto)
        {
            try
            {
                var nameUnique = await IsRoomNameUniqueAsync(createDto.RoomName);
                if (!nameUnique.Success || !nameUnique.Data)
                {
                    return ServiceResult<RoomResponseDto>.ErrorResult("Bu adda otaq artıq mövcuddur");
                }

                var room = _mapper.Map<Room>(createDto);
                var createdRoom = await _roomRepository.AddAsync(room);
                await _roomRepository.SaveChangesAsync();
                if (createDto.Images != null && createDto.Images.Any())
                {
                    List<Images> imageList = new List<Images>();

                    foreach (var file in createDto.Images)
                    {
                        var image = new Images
                        {
                            ImageUrl = file.FileUpload("wwwroot/Images", 5), 
                            RoomId = createdRoom.Id,
                            AltText = createDto.RoomName, 
                            DisplayOrder = 1,
                            IsMain = false 
                        };

                        imageList.Add(image);
                    }

                    await _imageRepository.AddRangeAsync(imageList);
                    await _imageRepository.SaveChangesAsync();
                }

                var roomDto = _mapper.Map<RoomResponseDto>(createdRoom);

                _logger.LogInformation("Room created successfully: {RoomName}", createDto.RoomName);
                return ServiceResult<RoomResponseDto>.SuccessResult(roomDto, "Otaq uğurla yaradıldı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating room: {RoomName}", createDto.RoomName);
                return ServiceResult<RoomResponseDto>.ErrorResult("Otaq yaradılmadı");
            }
        }

        public async Task<ServiceResult<RoomResponseDto>> UpdateRoomAsync(int id, RoomUpdateDto updateDto)
        {
            try
            {
                var room = await _roomRepository.GetByIdAsync(id);
                if (room == null)
                {
                    return ServiceResult<RoomResponseDto>.ErrorResult("Otaq tapılmadı");
                }

                var nameUnique = await IsRoomNameUniqueAsync(updateDto.RoomName, id);
                if (!nameUnique.Success || !nameUnique.Data)
                {
                    return ServiceResult<RoomResponseDto>.ErrorResult("Bu adda otaq artıq mövcuddur");
                }

                _mapper.Map(updateDto, room);
                _roomRepository.Update(room);
                await _roomRepository.SaveChangesAsync();

                var roomDto = _mapper.Map<RoomResponseDto>(room);

                _logger.LogInformation("Room updated successfully: {RoomId}", id);
                return ServiceResult<RoomResponseDto>.SuccessResult(roomDto, "Otaq uğurla yeniləndi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating room: {RoomId}", id);
                return ServiceResult<RoomResponseDto>.ErrorResult("Otaq yenilənmədi");
            }
        }

        public async Task<ServiceResult> DeleteRoomAsync(int id)
        {
            try
            {
                var room = await _roomRepository.GetByIdAsync(id);
                if (room == null)
                {
                    return ServiceResult.ErrorResult("Otaq tapılmadı");
                }

                var hasActiveReservations = await _reservationRepository.AnyAsync(r =>
                    r.RoomId == id && r.EndDate > DateTime.Now);

                if (hasActiveReservations)
                {
                    return ServiceResult.ErrorResult("Aktiv rezervasiyası olan otaq silinə bilməz");
                }

                _roomRepository.SoftDelete(room);
                await _roomRepository.SaveChangesAsync();

                _logger.LogInformation("Room deleted successfully: {RoomId}", id);
                return ServiceResult.SuccessResult("Otaq uğurla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room: {RoomId}", id);
                return ServiceResult.ErrorResult("Otaq silinmədi");
            }
        }

        #endregion

        #region Search and Filter Operations

        public async Task<ServiceResult<(IEnumerable<RoomResponseDto> Rooms, int TotalCount)>> SearchRoomsAsync(RoomSearchDto searchDto)
        {
            try
            {
                var query = _roomRepository.GetAll(true, "Images");

                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    query = query.Where(r => r.RoomName.Contains(searchDto.SearchTerm) ||
                                           r.Description.Contains(searchDto.SearchTerm));
                }

                if (searchDto.RoomType.HasValue)
                {
                    query = query.Where(r => r.Role == searchDto.RoomType.Value);
                }

                if (searchDto.MinPrice.HasValue)
                {
                    query = query.Where(r => r.Price >= searchDto.MinPrice.Value);
                }

                if (searchDto.MaxPrice.HasValue)
                {
                    query = query.Where(r => r.Price <= searchDto.MaxPrice.Value);
                }

                if (searchDto.MinCapacity.HasValue)
                {
                    query = query.Where(r => r.Capacity >= searchDto.MinCapacity.Value);
                }

                if (searchDto.MaxCapacity.HasValue)
                {
                    query = query.Where(r => r.Capacity <= searchDto.MaxCapacity.Value);
                }

                if (searchDto.HasWifi.HasValue)
                {
                    query = query.Where(r => r.Wifi == searchDto.HasWifi.Value);
                }

                query = searchDto.SortBy.ToLower() switch
                {
                    "price" => searchDto.IsDescending ? query.OrderByDescending(r => r.Price) : query.OrderBy(r => r.Price),
                    "capacity" => searchDto.IsDescending ? query.OrderByDescending(r => r.Capacity) : query.OrderBy(r => r.Capacity),
                    "roomname" => searchDto.IsDescending ? query.OrderByDescending(r => r.RoomName) : query.OrderBy(r => r.RoomName),
                    _ => searchDto.IsDescending ? query.OrderByDescending(r => r.CreatedDate) : query.OrderBy(r => r.CreatedDate)
                };

                var totalCount = await query.CountAsync();
                var rooms = await query.Skip((searchDto.Page - 1) * searchDto.PageSize)
                                     .Take(searchDto.PageSize)
                                     .ToListAsync();

                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);

                return ServiceResult<(IEnumerable<RoomResponseDto>, int)>.SuccessResult(
                    (roomDtos, totalCount),
                    "Axtarış nəticələri uğurla əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching rooms");
                return ServiceResult<(IEnumerable<RoomResponseDto>, int)>.ErrorResult("Axtarış zamanı xəta baş verdi");
            }
        }

        public async Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetRoomsByTypeAsync(RoomType roomType)
        {
            try
            {
                var rooms = await _roomRepository.GetByConditionAsync(r => r.Role == roomType, true, "Images");
                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);

                return ServiceResult<IEnumerable<RoomResponseDto>>.SuccessResult(
                    roomDtos,
                    $"{roomType} tipli otaqlar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms by type: {RoomType}", roomType);
                return ServiceResult<IEnumerable<RoomResponseDto>>.ErrorResult("Otaqlar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetRoomsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            try
            {
                var rooms = await _roomRepository.GetByRangeAsync(r => r.Price, minPrice, maxPrice, true);
                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);

                return ServiceResult<IEnumerable<RoomResponseDto>>.SuccessResult(
                    roomDtos,
                    "Qiymət aralığına uyğun otaqlar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms by price range: {MinPrice}-{MaxPrice}", minPrice, maxPrice);
                return ServiceResult<IEnumerable<RoomResponseDto>>.ErrorResult("Otaqlar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetRoomsByCapacityAsync(int minCapacity, int maxCapacity)
        {
            try
            {
                var rooms = await _roomRepository.GetByRangeAsync(r => r.Capacity, minCapacity, maxCapacity, true);
                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);

                return ServiceResult<IEnumerable<RoomResponseDto>>.SuccessResult(
                    roomDtos,
                    "Tutuma uyğun otaqlar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rooms by capacity: {MinCapacity}-{MaxCapacity}", minCapacity, maxCapacity);
                return ServiceResult<IEnumerable<RoomResponseDto>>.ErrorResult("Otaqlar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetWifiEnabledRoomsAsync()
        {
            try
            {
                var rooms = await _roomRepository.GetByConditionAsync(r => r.Wifi == true, true, "Images");
                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);

                return ServiceResult<IEnumerable<RoomResponseDto>>.SuccessResult(
                    roomDtos,
                    "WiFi olan otaqlar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wifi enabled rooms");
                return ServiceResult<IEnumerable<RoomResponseDto>>.ErrorResult("WiFi olan otaqlar əldə edilmədi");
            }
        }

        #endregion

        #region Availability Operations

        public async Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetAvailableRoomsAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var bookedRoomIds = await _reservationRepository.GetDistinctAsync(
                    r => r.StartDate < endDate && r.EndDate > startDate,
                    r => r.RoomId
                );

                var availableRooms = await _roomRepository.GetByConditionAsync(
                    r => !bookedRoomIds.Contains(r.Id),
                    true,
                    "Images"
                );

                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(availableRooms);

                return ServiceResult<IEnumerable<RoomResponseDto>>.SuccessResult(
                    roomDtos,
                    "Boş otaqlar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available rooms for dates: {StartDate}-{EndDate}", startDate, endDate);
                return ServiceResult<IEnumerable<RoomResponseDto>>.ErrorResult("Boş otaqlar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<bool>> IsRoomAvailableAsync(int roomId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var hasConflict = await _reservationRepository.AnyAsync(r =>
                    r.RoomId == roomId &&
                    r.StartDate < endDate &&
                    r.EndDate > startDate
                );

                return ServiceResult<bool>.SuccessResult(!hasConflict,
                    hasConflict ? "Otaq bu tarixlərdə doludur" : "Otaq boşdur");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room availability: {RoomId}", roomId);
                return ServiceResult<bool>.ErrorResult("Otağın mövcudluğu yoxlana bilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetMostPopularRoomsAsync(int count = 10)
        {
            try
            {
                var reservations = await _reservationRepository.GetAllAsync();
                var popularRoomIds = reservations
                    .GroupBy(r => r.RoomId)
                    .OrderByDescending(g => g.Count())
                    .Take(count)
                    .Select(g => g.Key)
                    .ToList();

                var rooms = await _roomRepository.GetByIdsAsync(popularRoomIds, true, "Images");
                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(rooms);

                return ServiceResult<IEnumerable<RoomResponseDto>>.SuccessResult(
                    roomDtos,
                    "Ən məşhur otaqlar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting most popular rooms");
                return ServiceResult<IEnumerable<RoomResponseDto>>.ErrorResult("Məşhur otaqlar əldə edilmədi");
            }
        }

        #endregion

        #region Image Management

        public async Task<ServiceResult<ImageResponseDto>> AddRoomImageAsync(ImageCreateDto imageDto)
        {
            try
            {
                var roomExists = await _roomRepository.ExistsByIdAsync(imageDto.RoomId);
                if (!roomExists)
                {
                    return ServiceResult<ImageResponseDto>.ErrorResult("Otaq tapılmadı");
                }

                var image = _mapper.Map<Images>(imageDto);
                var createdImage = await _imageRepository.AddAsync(image);
                await _imageRepository.SaveChangesAsync();

                var imageResponseDto = _mapper.Map<ImageResponseDto>(createdImage);

                return ServiceResult<ImageResponseDto>.SuccessResult(
                    imageResponseDto,
                    "Şəkil uğurla əlavə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding room image for room: {RoomId}", imageDto.RoomId);
                return ServiceResult<ImageResponseDto>.ErrorResult("Şəkil əlavə edilmədi");
            }
        }

        public async Task<ServiceResult> DeleteRoomImageAsync(int imageId)
        {
            try
            {
                var image = await _imageRepository.GetByIdAsync(imageId);
                if (image == null)
                {
                    return ServiceResult.ErrorResult("Şəkil tapılmadı");
                }

                _imageRepository.SoftDelete(image);
                await _imageRepository.SaveChangesAsync();

                return ServiceResult.SuccessResult("Şəkil uğurla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting room image: {ImageId}", imageId);
                return ServiceResult.ErrorResult("Şəkil silinmədi");
            }
        }

        public async Task<ServiceResult> SetMainImageAsync(int roomId, int imageId)
        {
            try
            {
                var roomImages = await _imageRepository.GetByConditionAsync(i => i.RoomId == roomId);
                foreach (var img in roomImages)
                {
                    img.IsMain = false;
                    _imageRepository.Update(img);
                }

                var mainImage = await _imageRepository.GetByIdAsync(imageId);
                if (mainImage == null || mainImage.RoomId != roomId)
                {
                    return ServiceResult.ErrorResult("Şəkil tapılmadı və ya bu otağa aid deyil");
                }

                mainImage.IsMain = true;
                _imageRepository.Update(mainImage);
                await _imageRepository.SaveChangesAsync();

                return ServiceResult.SuccessResult("Əsas şəkil təyin edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting main image: {ImageId} for room: {RoomId}", imageId, roomId);
                return ServiceResult.ErrorResult("Əsas şəkil təyin edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ImageResponseDto>>> GetRoomImagesAsync(int roomId)
        {
            try
            {
                var images = await _imageRepository.GetByConditionAsync(i => i.RoomId == roomId);
                var imageDtos = _mapper.Map<IEnumerable<ImageResponseDto>>(images);

                return ServiceResult<IEnumerable<ImageResponseDto>>.SuccessResult(
                    imageDtos,
                    "Otaq şəkilləri əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room images for room: {RoomId}", roomId);
                return ServiceResult<IEnumerable<ImageResponseDto>>.ErrorResult("Şəkillər əldə edilmədi");
            }
        }

        #endregion

        #region Statistics and Analytics

        public async Task<ServiceResult<object>> GetRoomStatisticsAsync(int roomId)
        {
            try
            {
                var room = await _roomRepository.GetByIdAsync(roomId);
                if (room == null)
                {
                    return ServiceResult<object>.ErrorResult("Otaq tapılmadı");
                }

                var totalReservations = await _reservationRepository.CountAsync(r => r.RoomId == roomId);
                var activeReservations = await _reservationRepository.CountAsync(r =>
                    r.RoomId == roomId && r.EndDate > DateTime.Now);

                var stats = new
                {
                    RoomId = roomId,
                    RoomName = room.RoomName,
                    TotalReservations = totalReservations,
                    ActiveReservations = activeReservations,
                    CompletedReservations = totalReservations - activeReservations
                };

                return ServiceResult<object>.SuccessResult(stats, "Otaq statistikaları əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting room statistics: {RoomId}", roomId);
                return ServiceResult<object>.ErrorResult("Statistikalar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<object>> GetOverallRoomStatisticsAsync()
        {
            try
            {
                var totalRooms = await _roomRepository.CountAsync();
                var totalReservations = await _reservationRepository.CountAsync();
                var averagePrice = await _roomRepository.AverageAsync(r => r.Price);

                var roomsByType = await _roomRepository.GroupByAsync<RoomType, object>(
                    r => r.Role,
                    g => new { RoomType = g.Key, Count = g.Count() }
                );

                var stats = new
                {
                    TotalRooms = totalRooms,
                    TotalReservations = totalReservations,
                    AveragePrice = Math.Round(averagePrice, 2),
                    RoomsByType = roomsByType
                };

                return ServiceResult<object>.SuccessResult(stats, "Ümumi statistikalar əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting overall room statistics");
                return ServiceResult<object>.ErrorResult("Statistikalar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<RoomResponseDto>>> GetLeastPopularRoomsAsync(int count = 10)
        {
            try
            {
                var reservations = await _reservationRepository.GetAllAsync();
                var roomBookingCounts = reservations
                    .GroupBy(r => r.RoomId)
                    .ToDictionary(g => g.Key, g => g.Count());

                var allRooms = await _roomRepository.GetAllAsync(true, "Images");
                var leastPopularRooms = allRooms
                    .OrderBy(r => roomBookingCounts.GetValueOrDefault(r.Id, 0))
                    .Take(count);

                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(leastPopularRooms);

                return ServiceResult<IEnumerable<RoomResponseDto>>.SuccessResult(
                    roomDtos,
                    "Ən az populyar otaqlar əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting least popular rooms");
                return ServiceResult<IEnumerable<RoomResponseDto>>.ErrorResult("Ən az populyar otaqlar əldə edilmədi");
            }
        }
        public async Task<ServiceResult<IEnumerable<RoomResponseDto>>> SearchAvailableRoomsByTypeAsync(DateTime startDate, DateTime endDate, RoomType? roomType = null)
        {
            try
            {
                
                var bookedRoomIds = await _reservationRepository.GetDistinctAsync(
                    r => r.StartDate < endDate && r.EndDate > startDate,
                    r => r.RoomId
                );

            
                var availableRoomsQuery = _roomRepository.GetAll(true, "Images")
                    .Where(r => !bookedRoomIds.Contains(r.Id));

              
                if (roomType.HasValue)
                {
                    availableRoomsQuery = availableRoomsQuery.Where(r => r.Role == roomType.Value);
                }

                var availableRooms = await availableRoomsQuery.ToListAsync();
                var roomDtos = _mapper.Map<IEnumerable<RoomResponseDto>>(availableRooms);

                var message = roomType.HasValue
                    ? $"{roomType.Value} tipli boş otaqlar əldə edildi ({availableRooms.Count()} ədəd)"
                    : $"Boş otaqlar əldə edildi ({availableRooms.Count()} ədəd)";

                return ServiceResult<IEnumerable<RoomResponseDto>>.SuccessResult(
                    roomDtos,
                    message
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching available rooms by type for dates: {StartDate}-{EndDate}, Type: {RoomType}",
                    startDate, endDate, roomType);
                return ServiceResult<IEnumerable<RoomResponseDto>>.ErrorResult("Boş otaqlar tapıla bilmədi");
            }
        }

        #endregion

        #region Validation Helpers

        public async Task<ServiceResult<bool>> IsRoomNameUniqueAsync(string roomName, int? excludeId = null)
        {
            try
            {
                var exists = excludeId.HasValue
                    ? await _roomRepository.AnyAsync(r => r.RoomName == roomName && r.Id != excludeId.Value)
                    : await _roomRepository.AnyAsync(r => r.RoomName == roomName);

                return ServiceResult<bool>.SuccessResult(!exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room name uniqueness: {RoomName}", roomName);
                return ServiceResult<bool>.ErrorResult("Ad yoxlanıla bilmədi");
            }
        }

        public async Task<ServiceResult<bool>> RoomExistsAsync(int roomId)
        {
            try
            {
                var exists = await _roomRepository.ExistsByIdAsync(roomId);
                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking room existence: {RoomId}", roomId);
                return ServiceResult<bool>.ErrorResult("Otağın mövcudluğu yoxlana bilmədi");
            }
        }

        #endregion
    }
}