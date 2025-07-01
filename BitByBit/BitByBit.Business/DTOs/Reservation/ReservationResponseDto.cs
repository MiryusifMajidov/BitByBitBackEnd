// ReservationDto.cs
using BitByBit.Business.DTOs.Room;
using BitByBit.Business.DTOs.User;
using System.ComponentModel.DataAnnotations;

namespace BitByBit.Business.DTOs.Reservation
{
    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalNights { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }


        public RoomResponseDto? Room { get; set; }
        public UserResponseDto? User { get; set; }
    }

    public class ReservationCreateDto
    {
        [Required(ErrorMessage = "Otaq ID-si tələb olunur")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Giriş tarixi tələb olunur")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Çıxış tarixi tələb olunur")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

   
        public bool IsValidDateRange()
        {
            return StartDate < EndDate && StartDate >= DateTime.Today;
        }

        public int CalculateTotalNights()
        {
            return (EndDate - StartDate).Days;
        }
    }

    public class ReservationUpdateDto
    {
        [Required(ErrorMessage = "Giriş tarixi tələb olunur")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Çıxış tarixi tələb olunur")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

 
        public bool IsValidDateRange()
        {
            return StartDate < EndDate && StartDate >= DateTime.Today;
        }

        public int CalculateTotalNights()
        {
            return (EndDate - StartDate).Days;
        }
    }

    public class ReservationSearchDto
    {
        public string? UserId { get; set; }
        public int? RoomId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CreatedAfter { get; set; }
        public DateTime? CreatedBefore { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedDate";
        public bool IsDescending { get; set; } = true;
    }

    public class AvailabilityCheckDto
    {
        [Required(ErrorMessage = "Otaq ID-si tələb olunur")]
        public int RoomId { get; set; }

        [Required(ErrorMessage = "Giriş tarixi tələb olunur")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Çıxış tarixi tələb olunur")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        public int? ExcludeReservationId { get; set; } 
    }

    public class AvailabilityResponseDto
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<ReservationResponseDto> ConflictingReservations { get; set; } = new();
    }

    public class UserReservationSummaryDto
    {
        public int TotalReservations { get; set; }
        public int UpcomingReservations { get; set; }
        public int CompletedReservations { get; set; }
        public decimal TotalAmountSpent { get; set; }
        public DateTime? LastReservationDate { get; set; }
        public List<ReservationResponseDto> RecentReservations { get; set; } = new();
    }
}