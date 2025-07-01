// RoomDto.cs
using BitByBit.Entities.Enums;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace BitByBit.Business.DTOs.Room
{
    public class RoomResponseDto
    {
        public int Id { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public RoomType Role { get; set; }
        public string RoomTypeName => Role.ToString();
        public int Capacity { get; set; }
        public int RoomCount { get; set; }
        public int BathCount { get; set; }
        public bool Wifi { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<ImageResponseDto> Images { get; set; } = new();
    }

    public class RoomCreateDto
    {
        [Required(ErrorMessage = "Otaq adı tələb olunur")]
        [StringLength(100, ErrorMessage = "Otaq adı maksimum 100 simvol ola bilər")]
        public string RoomName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Təsvir maksimum 500 simvol ola bilər")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Otaq tipi tələb olunur")]
        public RoomType Role { get; set; } = RoomType.Standart;

        [Required(ErrorMessage = "Tutum tələb olunur")]
        [Range(1, 20, ErrorMessage = "Tutum 1 ilə 20 arasında olmalıdır")]
        public int Capacity { get; set; } = 2;

        [Required(ErrorMessage = "Otaq sayı tələb olunur")]
        [Range(1, 10, ErrorMessage = "Otaq sayı 1 ilə 10 arasında olmalıdır")]
        public int RoomCount { get; set; } = 1;

        [Required(ErrorMessage = "Hamam sayı tələb olunur")]
        [Range(1, 5, ErrorMessage = "Hamam sayı 1 ilə 5 arasında olmalıdır")]
        public int BathCount { get; set; } = 1;

        public bool Wifi { get; set; } = false;

        [Required(ErrorMessage = "Qiymət tələb olunur")]
        [Range(0.01, 99999.99, ErrorMessage = "Qiymət 0.01 ilə 99999.99 arasında olmalıdır")]
        public decimal Price { get; set; }

        [MaxFileSize(5 * 1024 * 1024, ErrorMessage = "Each image cannot be larger than 5 MB.")]
        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".gif", ".jfif" }, ErrorMessage = "Only JPG, JPEG, PNG, and GIF files are allowed.")]
        public List<IFormFile>? Images { get; set; }
    }

    public class RoomUpdateDto
    {
        [Required(ErrorMessage = "Otaq adı tələb olunur")]
        [StringLength(100, ErrorMessage = "Otaq adı maksimum 100 simvol ola bilər")]
        public string RoomName { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Təsvir maksimum 500 simvol ola bilər")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Otaq tipi tələb olunur")]
        public RoomType Role { get; set; }

        [Required(ErrorMessage = "Tutum tələb olunur")]
        [Range(1, 20, ErrorMessage = "Tutum 1 ilə 20 arasında olmalıdır")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Otaq sayı tələb olunur")]
        [Range(1, 10, ErrorMessage = "Otaq sayı 1 ilə 10 arasında olmalıdır")]
        public int RoomCount { get; set; }

        [Required(ErrorMessage = "Hamam sayı tələb olunur")]
        [Range(1, 5, ErrorMessage = "Hamam sayı 1 ilə 5 arasında olmalıdır")]
        public int BathCount { get; set; }

        public bool Wifi { get; set; }

        [Required(ErrorMessage = "Qiymət tələb olunur")]
        [Range(0.01, 99999.99, ErrorMessage = "Qiymət 0.01 ilə 99999.99 arasında olmalıdır")]
        public decimal Price { get; set; }
    }

    public class RoomSearchDto
    {
        public string? SearchTerm { get; set; }
        public RoomType? RoomType { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public int? MinCapacity { get; set; }
        public int? MaxCapacity { get; set; }
        public bool? HasWifi { get; set; }
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string SortBy { get; set; } = "CreatedDate";
        public bool IsDescending { get; set; } = true;
    }

    public class ImageResponseDto
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public bool IsMain { get; set; }
    }

    public class ImageCreateDto
    {
        [Required(ErrorMessage = "Şəkil URL-i tələb olunur")]
        [Url(ErrorMessage = "Düzgün URL formatı daxil edin")]
        public string ImageUrl { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Alt mətn maksimum 100 simvol ola bilər")]
        public string AltText { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Göstəriş sırası 1 ilə 100 arasında olmalıdır")]
        public int DisplayOrder { get; set; } = 1;

        public bool IsMain { get; set; } = false;

        [Required(ErrorMessage = "Otaq ID-si tələb olunur")]
        public int RoomId { get; set; }
    }
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxSize;

        public MaxFileSizeAttribute(int maxSize)
        {
            _maxSize = maxSize;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxSize)
                {
                    return new ValidationResult($"File size cannot exceed {_maxSize / (1024 * 1024)} MB.");
                }
            }
            return ValidationResult.Success;
        }
    }
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var files = value as List<IFormFile>;
            if (files != null)
            {
                foreach (var file in files)
                {
                    var extension = System.IO.Path.GetExtension(file.FileName).ToLower();
                    if (!_extensions.Contains(extension))
                    {
                        return new ValidationResult($"Only {string.Join(", ", _extensions)} file types are allowed.");
                    }
                }
            }
            return ValidationResult.Success;
        }
    }
}