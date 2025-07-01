using AutoMapper;
using BitByBit.Business.DTOs.Common;
using BitByBit.Business.DTOs.Services;
using BitByBit.Business.Extensions;
using BitByBit.Business.Services.Interfaces;
using BitByBit.DataAccess.Repository.Interfaces;
using BitByBit.Entities.Enums;
using BitByBit.Entities.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BitByBit.Business.Services.Implementations
{
    public class ServicesService : IServicesService
    {
        private readonly IRepository<Service> _servicesRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<ServicesService> _logger;

        public ServicesService(
            IRepository<Service> servicesRepository,
            IMapper mapper,
            ILogger<ServicesService> logger)
        {
            _servicesRepository = servicesRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ServiceResult<(IEnumerable<ServicesResponseDto> Services, int TotalCount)>> GetAllServicesAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var (services, totalCount) = await _servicesRepository.GetPagedWithCountAsync(page, pageSize);
                var serviceDtos = _mapper.Map<IEnumerable<ServicesResponseDto>>(services);

                return ServiceResult<(IEnumerable<ServicesResponseDto>, int)>.SuccessResult(
                    (serviceDtos, totalCount),
                    "Xidmətlər uğurla əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all services");
                return ServiceResult<(IEnumerable<ServicesResponseDto>, int)>.ErrorResult("Xidmətlər əldə edilmədi");
            }
        }

        public async Task<ServiceResult<ServicesResponseDto>> GetServiceByIdAsync(int id)
        {
            try
            {
                var service = await _servicesRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return ServiceResult<ServicesResponseDto>.ErrorResult("Xidmət tapılmadı");
                }

                var serviceDto = _mapper.Map<ServicesResponseDto>(service);
                return ServiceResult<ServicesResponseDto>.SuccessResult(serviceDto, "Xidmət uğurla əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service by id: {ServiceId}", id);
                return ServiceResult<ServicesResponseDto>.ErrorResult("Xidmət əldə edilmədi");
            }
        }

        public async Task<ServiceResult<ServicesResponseDto>> CreateServiceAsync(ServicesCreateDto createDto)
        {
            try
            {
                var service = _mapper.Map<Service>(createDto);

                
                if (createDto.IconUrl != null)
                {
                    service.IconUrl = createDto.IconUrl.FileUpload("wwwroot/Icons", 5); 
                }

                var createdService = await _servicesRepository.AddAsync(service);
                await _servicesRepository.SaveChangesAsync();

                var serviceDto = _mapper.Map<ServicesResponseDto>(createdService);

                _logger.LogInformation("Service created successfully: {ServiceName}", createDto.ServiceName);
                return ServiceResult<ServicesResponseDto>.SuccessResult(serviceDto, "Xidmət uğurla yaradıldı");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service: {ServiceName}", createDto.ServiceName);
                return ServiceResult<ServicesResponseDto>.ErrorResult("Xidmət yaradılmadı");
            }
        }


        public async Task<ServiceResult<ServicesResponseDto>> UpdateServiceAsync(int id, ServicesUpdateDto updateDto)
        {
            try
            {
                var service = await _servicesRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return ServiceResult<ServicesResponseDto>.ErrorResult("Xidmət tapılmadı");
                }

                _mapper.Map(updateDto, service);
                _servicesRepository.Update(service);
                await _servicesRepository.SaveChangesAsync();

                var serviceDto = _mapper.Map<ServicesResponseDto>(service);

                _logger.LogInformation("Service updated successfully: {ServiceId}", id);
                return ServiceResult<ServicesResponseDto>.SuccessResult(serviceDto, "Xidmət uğurla yeniləndi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service: {ServiceId}", id);
                return ServiceResult<ServicesResponseDto>.ErrorResult("Xidmət yenilənmədi");
            }
        }

        public async Task<ServiceResult> DeleteServiceAsync(int id)
        {
            try
            {
                var service = await _servicesRepository.GetByIdAsync(id);
                if (service == null)
                {
                    return ServiceResult.ErrorResult("Xidmət tapılmadı");
                }

                _servicesRepository.SoftDelete(service);
                await _servicesRepository.SaveChangesAsync();

                _logger.LogInformation("Service deleted successfully: {ServiceId}", id);
                return ServiceResult.SuccessResult("Xidmət uğurla silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service: {ServiceId}", id);
                return ServiceResult.ErrorResult("Xidmət silinmədi");
            }
        }

        public async Task<ServiceResult<(IEnumerable<ServicesResponseDto> Services, int TotalCount)>> SearchServicesAsync(ServicesSearchDto searchDto)
        {
            try
            {
                var query = _servicesRepository.GetAll();

                if (!string.IsNullOrEmpty(searchDto.SearchTerm))
                {
                    query = query.Where(s => s.ServiceName.Contains(searchDto.SearchTerm));
                }

                if (searchDto.RoomType.HasValue)
                {
                    query = query.Where(s => s.Role == searchDto.RoomType.Value);
                }

                var totalCount = await query.CountAsync();
                var services = await query.Skip((searchDto.Page - 1) * searchDto.PageSize)
                                         .Take(searchDto.PageSize)
                                         .ToListAsync();

                var serviceDtos = _mapper.Map<IEnumerable<ServicesResponseDto>>(services);

                return ServiceResult<(IEnumerable<ServicesResponseDto>, int)>.SuccessResult(
                    (serviceDtos, totalCount),
                    "Axtarış nəticələri uğurla əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching services");
                return ServiceResult<(IEnumerable<ServicesResponseDto>, int)>.ErrorResult("Axtarış zamanı xəta baş verdi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ServicesResponseDto>>> GetServicesByRoomTypeAsync(RoomType roomType)
        {
            try
            {
                var services = await _servicesRepository.GetByConditionAsync(s => s.Role == roomType);
                var serviceDtos = _mapper.Map<IEnumerable<ServicesResponseDto>>(services);

                return ServiceResult<IEnumerable<ServicesResponseDto>>.SuccessResult(
                    serviceDtos,
                    $"{roomType} tipli xidmətlər əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services by room type: {RoomType}", roomType);
                return ServiceResult<IEnumerable<ServicesResponseDto>>.ErrorResult("Xidmətlər əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ServicesResponseDto>>> SearchServicesByNameAsync(string searchTerm)
        {
            try
            {
                var services = await _servicesRepository.GetByConditionAsync(s => s.ServiceName.Contains(searchTerm));
                var serviceDtos = _mapper.Map<IEnumerable<ServicesResponseDto>>(services);

                return ServiceResult<IEnumerable<ServicesResponseDto>>.SuccessResult(
                    serviceDtos,
                    "Xidmətlər tapıldı"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching services by name: {SearchTerm}", searchTerm);
                return ServiceResult<IEnumerable<ServicesResponseDto>>.ErrorResult("Axtarış zamanı xəta baş verdi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ServicesResponseDto>>> GetServicesWithIconsAsync()
        {
            try
            {
                var services = await _servicesRepository.GetByConditionAsync(s => !string.IsNullOrEmpty(s.IconUrl));
                var serviceDtos = _mapper.Map<IEnumerable<ServicesResponseDto>>(services);

                return ServiceResult<IEnumerable<ServicesResponseDto>>.SuccessResult(
                    serviceDtos,
                    "İkon olan xidmətlər əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting services with icons");
                return ServiceResult<IEnumerable<ServicesResponseDto>>.ErrorResult("İkonlu xidmətlər əldə edilmədi");
            }
        }

        public async Task<ServiceResult<Dictionary<RoomType, IEnumerable<ServicesResponseDto>>>> GetServicesGroupedByRoomTypeAsync()
        {
            try
            {
                var services = await _servicesRepository.GetAllAsync();
                var serviceDtos = _mapper.Map<IEnumerable<ServicesResponseDto>>(services);

                var groupedServices = serviceDtos.GroupBy(s => s.Role)
                                                .ToDictionary(g => g.Key, g => g.AsEnumerable());

                return ServiceResult<Dictionary<RoomType, IEnumerable<ServicesResponseDto>>>.SuccessResult(
                    groupedServices,
                    "Xidmətlər qruplaşdırıldı"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error grouping services by room type");
                return ServiceResult<Dictionary<RoomType, IEnumerable<ServicesResponseDto>>>.ErrorResult("Qruplaşdırma zamanı xəta baş verdi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ServicesResponseDto>>> GetLatestServicesAsync(int count = 10)
        {
            try
            {
                var services = await _servicesRepository.GetLatestAsync(count);
                var serviceDtos = _mapper.Map<IEnumerable<ServicesResponseDto>>(services);

                return ServiceResult<IEnumerable<ServicesResponseDto>>.SuccessResult(
                    serviceDtos,
                    "Ən yeni xidmətlər əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest services");
                return ServiceResult<IEnumerable<ServicesResponseDto>>.ErrorResult("Yeni xidmətlər əldə edilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ServicesResponseDto>>> GetOldestServicesAsync(int count = 10)
        {
            try
            {
                var services = await _servicesRepository.GetOldestAsync(count);
                var serviceDtos = _mapper.Map<IEnumerable<ServicesResponseDto>>(services);

                return ServiceResult<IEnumerable<ServicesResponseDto>>.SuccessResult(
                    serviceDtos,
                    "Ən köhnə xidmətlər əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting oldest services");
                return ServiceResult<IEnumerable<ServicesResponseDto>>.ErrorResult("Köhnə xidmətlər əldə edilmədi");
            }
        }

        public async Task<ServiceResult<object>> GetServiceStatisticsAsync()
        {
            try
            {
                var totalServices = await _servicesRepository.CountAsync();
                var stats = new { TotalServices = totalServices };

                return ServiceResult<object>.SuccessResult(stats, "Statistikalar əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service statistics");
                return ServiceResult<object>.ErrorResult("Statistikalar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<Dictionary<RoomType, int>>> GetServiceCountByRoomTypeAsync()
        {
            try
            {
                var services = await _servicesRepository.GetAllAsync();
                var counts = services.GroupBy(s => s.Role)
                                   .ToDictionary(g => g.Key, g => g.Count());

                return ServiceResult<Dictionary<RoomType, int>>.SuccessResult(
                    counts,
                    "Otaq tipinə görə xidmət sayları əldə edildi"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service count by room type");
                return ServiceResult<Dictionary<RoomType, int>>.ErrorResult("Saylar əldə edilmədi");
            }
        }

        public async Task<ServiceResult<object>> GetIconStatisticsAsync()
        {
            try
            {
                var totalServices = await _servicesRepository.CountAsync();
                var withIcons = await _servicesRepository.CountAsync(s => !string.IsNullOrEmpty(s.IconUrl));

                var stats = new { TotalServices = totalServices, WithIcons = withIcons };

                return ServiceResult<object>.SuccessResult(stats, "İkon statistikaları əldə edildi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting icon statistics");
                return ServiceResult<object>.ErrorResult("İkon statistikaları əldə edilmədi");
            }
        }

        public async Task<ServiceResult<bool>> IsServiceNameUniqueAsync(string serviceName, int? excludeId = null)
        {
            try
            {
                var exists = excludeId.HasValue
                    ? await _servicesRepository.AnyAsync(s => s.ServiceName == serviceName && s.Id != excludeId.Value)
                    : await _servicesRepository.AnyAsync(s => s.ServiceName == serviceName);

                return ServiceResult<bool>.SuccessResult(!exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking service name uniqueness: {ServiceName}", serviceName);
                return ServiceResult<bool>.ErrorResult("Ad yoxlanıla bilmədi");
            }
        }

        public async Task<ServiceResult<bool>> ServiceExistsAsync(int serviceId)
        {
            try
            {
                var exists = await _servicesRepository.ExistsByIdAsync(serviceId);
                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking service existence: {ServiceId}", serviceId);
                return ServiceResult<bool>.ErrorResult("Xidmətin mövcudluğu yoxlana bilmədi");
            }
        }

        public async Task<ServiceResult<bool>> ValidateIconUrlAsync(string iconUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(iconUrl))
                    return ServiceResult<bool>.SuccessResult(true);

                var isValid = Uri.TryCreate(iconUrl, UriKind.Absolute, out _);
                return ServiceResult<bool>.SuccessResult(isValid);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating icon URL: {IconUrl}", iconUrl);
                return ServiceResult<bool>.ErrorResult("URL yoxlanıla bilmədi");
            }
        }

        public async Task<ServiceResult<IEnumerable<ServicesResponseDto>>> CreateMultipleServicesAsync(IEnumerable<ServicesCreateDto> createDtos)
        {
            try
            {
                var services = _mapper.Map<IEnumerable<Service>>(createDtos);
                var createdServices = await _servicesRepository.AddRangeAsync(services);
                await _servicesRepository.SaveChangesAsync();

                var serviceDtos = _mapper.Map<IEnumerable<ServicesResponseDto>>(createdServices);

                return ServiceResult<IEnumerable<ServicesResponseDto>>.SuccessResult(
                    serviceDtos,
                    "Çoxlu xidmət uğurla yaradıldı"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating multiple services");
                return ServiceResult<IEnumerable<ServicesResponseDto>>.ErrorResult("Çoxlu xidmət yaradılmadı");
            }
        }

        public async Task<ServiceResult> DeleteServicesByRoomTypeAsync(RoomType roomType)
        {
            try
            {
                var services = await _servicesRepository.GetByConditionAsync(s => s.Role == roomType);
                foreach (var service in services)
                {
                    _servicesRepository.SoftDelete(service);
                }
                await _servicesRepository.SaveChangesAsync();

                return ServiceResult.SuccessResult("Xidmətlər silindi");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting services by room type: {RoomType}", roomType);
                return ServiceResult.ErrorResult("Xidmətlər silinmədi");
            }
        }
    }
}