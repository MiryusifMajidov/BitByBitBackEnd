// AutoMapperProfile.cs - Tam versiya
using AutoMapper;
using BitByBit.Business.DTOs.User;
using BitByBit.Business.DTOs.Room;
using BitByBit.Business.DTOs.Services;
using BitByBit.Business.DTOs.Reservation;
using BitByBit.Entities.Models;
using BitByBit.Entities.Enums;

namespace BitByBit.Business.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            #region User Mappings

            // User Entity mappings
            CreateMap<User, UserResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => src.EmailConfirmed))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.LastLoginDate));

            // RegisterDto to User mapping
            CreateMap<RegisterDto, User>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.EmailConfirmed, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => UserRole.User))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => UserStatus.Active))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.LastLoginDate, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
                .ForMember(dest => dest.SecurityStamp, opt => opt.Ignore())
                .ForMember(dest => dest.ConcurrencyStamp, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumberConfirmed, opt => opt.Ignore())
                .ForMember(dest => dest.TwoFactorEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnd, opt => opt.Ignore())
                .ForMember(dest => dest.LockoutEnabled, opt => opt.Ignore())
                .ForMember(dest => dest.AccessFailedCount, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedUserName, opt => opt.Ignore())
                .ForMember(dest => dest.NormalizedEmail, opt => opt.Ignore());

            // UpdateProfileDto to User mapping
            CreateMap<UpdateProfileDto, User>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.Now));

            #endregion

            #region Room Mappings

            // Room Entity to ResponseDto
            CreateMap<Room, RoomResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.RoomName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
                .ForMember(dest => dest.RoomCount, opt => opt.MapFrom(src => src.RoomCount))
                .ForMember(dest => dest.BathCount, opt => opt.MapFrom(src => src.BathCount))
                .ForMember(dest => dest.Wifi, opt => opt.MapFrom(src => src.Wifi))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

            // RoomCreateDto to Room Entity
            CreateMap<RoomCreateDto, Room>()
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.RoomName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
                .ForMember(dest => dest.RoomCount, opt => opt.MapFrom(src => src.RoomCount))
                .ForMember(dest => dest.BathCount, opt => opt.MapFrom(src => src.BathCount))
                .ForMember(dest => dest.Wifi, opt => opt.MapFrom(src => src.Wifi))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
             
                .ForMember(dest => dest.Images, opt => opt.Ignore()); // Images ayrıca handle edilir

            // RoomUpdateDto to Room Entity
            CreateMap<RoomUpdateDto, Room>()
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.RoomName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
                .ForMember(dest => dest.RoomCount, opt => opt.MapFrom(src => src.RoomCount))
                .ForMember(dest => dest.BathCount, opt => opt.MapFrom(src => src.BathCount))
                .ForMember(dest => dest.Wifi, opt => opt.MapFrom(src => src.Wifi))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.Images, opt => opt.Ignore());

            #endregion

            #region Services Mappings

            // Service Entity to ResponseDto
            CreateMap<Service, ServicesResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.ServiceName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.RoomTypeName, opt => opt.MapFrom(src => src.Role.ToString()))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate));

            // ServicesCreateDto to Service Entity
            CreateMap<ServicesCreateDto, Service>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.ServiceName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IconUrl, opt => opt.Ignore()); // File upload service-də handle edilir

            // ServicesUpdateDto to Service Entity
            CreateMap<ServicesUpdateDto, Service>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.ServiceName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IconUrl, opt => opt.MapFrom(src => src.IconUrl))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

            #endregion

            #region Reservation Mappings

            // Reservation Entity to ResponseDto
            CreateMap<Reservation, ReservationResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.TotalNights, opt => opt.MapFrom(src => src.TotalNights))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => CalculateTotalAmount(src)))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Room))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            // ReservationCreateDto to Reservation Entity
            CreateMap<ReservationCreateDto, Reservation>()
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Service-də set edilir
                .ForMember(dest => dest.TotalNights, opt => opt.Ignore()) // Service-də calculate edilir
                .ForMember(dest => dest.Room, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            // ReservationUpdateDto to Reservation Entity
            CreateMap<ReservationUpdateDto, Reservation>()
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
                .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.RoomId, opt => opt.Ignore()) // Dəyişdirilmir
                .ForMember(dest => dest.UserId, opt => opt.Ignore()) // Dəyişdirilmir
                .ForMember(dest => dest.TotalNights, opt => opt.Ignore()) // Service-də calculate edilir
                .ForMember(dest => dest.Room, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());

            #endregion

            #region Images Mappings

            // Images Entity to ResponseDto
            CreateMap<Images, ImageResponseDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.AltText, opt => opt.MapFrom(src => src.AltText))
                .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
                .ForMember(dest => dest.IsMain, opt => opt.MapFrom(src => src.IsMain));

            // ImageCreateDto to Images Entity
            CreateMap<ImageCreateDto, Images>()
                .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl))
                .ForMember(dest => dest.AltText, opt => opt.MapFrom(src => src.AltText))
                .ForMember(dest => dest.DisplayOrder, opt => opt.MapFrom(src => src.DisplayOrder))
                .ForMember(dest => dest.IsMain, opt => opt.MapFrom(src => src.IsMain))
                .ForMember(dest => dest.RoomId, opt => opt.MapFrom(src => src.RoomId))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Room, opt => opt.Ignore());

            #endregion

            #region Reverse Mappings (əgər lazımsa)

            // Reverse mappings - əksini yaratmaq üçün istifadə edilə bilər
            CreateMap<RoomResponseDto, Room>().ReverseMap();
            CreateMap<ServicesResponseDto, Service>().ReverseMap();
            CreateMap<ImageResponseDto, Images>().ReverseMap();

            #endregion
        }

        #region Helper Methods

        /// <summary>
        /// Rezervasiya üçün total amount hesablama
        /// </summary>
        /// <param name="reservation">Reservation entity</param>
        /// <returns>Total amount</returns>
        private static decimal CalculateTotalAmount(Reservation reservation)
        {
            // Əgər Room navigation property loaded deyilsə, 0 qaytar
            if (reservation.Room == null)
                return 0;

            return reservation.TotalNights * reservation.Room.Price;
        }

        /// <summary>
        /// Full name yaratmaq üçün helper method
        /// </summary>
        /// <param name="firstName">Ad</param>
        /// <param name="lastName">Soyad</param>
        /// <returns>Tam ad</returns>
        private static string CreateFullName(string firstName, string lastName)
        {
            return $"{firstName} {lastName}".Trim();
        }

        /// <summary>
        /// DateTime UTC-yə çevirmək üçün helper method
        /// </summary>
        /// <returns>UTC DateTime</returns>
        private static DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }

        #endregion
    }
}