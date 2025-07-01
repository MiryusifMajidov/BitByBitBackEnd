// ServicesDto.cs
using BitByBit.Entities.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BitByBit.Business.DTOs.Services
{
    public class ServicesResponseDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public RoomType Role { get; set; }
        public string RoomTypeName => Role.ToString();
        public DateTime CreatedDate { get; set; }
    }
    
    public class ServicesCreateDto
    {
        [Required(ErrorMessage = "Xidmət adı tələb olunur")]
        [StringLength(100, ErrorMessage = "Xidmət adı maksimum 100 simvol ola bilər")]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Təsvir maksimum 500 simvol ola bilər")]
        public string Description { get; set; } = string.Empty;

        [Url(ErrorMessage = "Düzgün URL formatı daxil edin")]
        [StringLength(255, ErrorMessage = "Icon URL maksimum 255 simvol ola bilər")]
        public IFormFile IconUrl { get; set; }

        [Required(ErrorMessage = "Otaq tipi tələb olunur")]
        public RoomType Role { get; set; } = RoomType.Standart;

    }

    public class ServicesUpdateDto
    {
        [Required(ErrorMessage = "Xidmət adı tələb olunur")]
        [StringLength(100, ErrorMessage = "Xidmət adı maksimum 100 simvol ola bilər")]
        public string ServiceName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Təsvir maksimum 500 simvol ola bilər")]
        public string Description { get; set; } = string.Empty;

        [Url(ErrorMessage = "Düzgün URL formatı daxil edin")]
        [StringLength(255, ErrorMessage = "Icon URL maksimum 255 simvol ola bilər")]
        public string IconUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Otaq tipi tələb olunur")]
        public RoomType Role { get; set; }
    }

    public class ServicesSearchDto
    {
        public string? SearchTerm { get; set; }
        public RoomType? RoomType { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "ServiceName";
        public bool IsDescending { get; set; } = false;
    }
}