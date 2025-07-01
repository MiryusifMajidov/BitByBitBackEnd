/*using BitByBit.Business.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using static System.Net.Mime.MediaTypeNames;

namespace BitByBit.Business.Services.Implementations
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;
        private readonly string _baseUploadPath;
        private readonly long _maxFileSize;
        private readonly string[] _allowedImageExtensions;
        private readonly string[] _allowedDocumentExtensions;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
            _baseUploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            _maxFileSize = 10 * 1024 * 1024; // 10MB
            _allowedImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            _allowedDocumentExtensions = new[] { ".pdf", ".doc", ".docx", ".txt", ".xls", ".xlsx" };

            // Create upload directory if it doesn't exist
            EnsureDirectoryExists(_baseUploadPath);
        }

        #region File Upload Operations

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "images")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("Fayl boş və ya mövcud deyil");
                }

                // Validate file
                ValidateImageFile(file);

                var uploadPath = Path.Combine(_baseUploadPath, folder);
                EnsureDirectoryExists(uploadPath);

                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Image uploaded successfully: {FileName}", fileName);
                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image: {FileName}", file?.FileName);
                throw;
            }
        }

        public async Task<string> UploadDocumentAsync(IFormFile file, string folder = "documents")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("Fayl boş və ya mövcud deyil");
                }

                // Validate file
                ValidateDocumentFile(file);

                var uploadPath = Path.Combine(_baseUploadPath, folder);
                EnsureDirectoryExists(uploadPath);

                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Document uploaded successfully: {FileName}", fileName);
                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading document: {FileName}", file?.FileName);
                throw;
            }
        }

        public async Task<List<string>> UploadMultipleImagesAsync(IFormFileCollection files, string folder = "images")
        {
            var uploadedFiles = new List<string>();

            try
            {
                foreach (var file in files)
                {
                    if (file != null && file.Length > 0)
                    {
                        var filePath = await UploadImageAsync(file, folder);
                        uploadedFiles.Add(filePath);
                    }
                }

                _logger.LogInformation("Multiple images uploaded successfully. Count: {Count}", uploadedFiles.Count);
                return uploadedFiles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading multiple images");

                // Clean up uploaded files on error
                foreach (var uploadedFile in uploadedFiles)
                {
                    try
                    {
                        await DeleteFileAsync(uploadedFile);
                    }
                    catch
                    {
                        // Ignore errors during cleanup
                    }
                }

                throw;
            }
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folder = "files")
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    throw new ArgumentException("Fayl boş və ya mövcud deyil");
                }

                // Basic validation
                if (file.Length > _maxFileSize)
                {
                    throw new ArgumentException($"Fayl ölçüsü {_maxFileSize / (1024 * 1024)}MB-dan böyük ola bilməz");
                }

                var uploadPath = Path.Combine(_baseUploadPath, folder);
                EnsureDirectoryExists(uploadPath);

                var fileName = GenerateUniqueFileName(file.FileName);
                var filePath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("File uploaded successfully: {FileName}", fileName);
                return $"/uploads/{folder}/{fileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file: {FileName}", file?.FileName);
                throw;
            }
        }

        #endregion

        #region File Delete Operations

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return false;
                }

                // Remove leading slash and convert to physical path
                var relativePath = filePath.TrimStart('/');
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                if (File.Exists(physicalPath))
                {
                    File.Delete(physicalPath);
                    _logger.LogInformation("File deleted successfully: {FilePath}", filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<bool> DeleteMultipleFilesAsync(IEnumerable<string> filePaths)
        {
            var allSuccess = true;

            foreach (var filePath in filePaths)
            {
                var success = await DeleteFileAsync(filePath);
                if (!success)
                {
                    allSuccess = false;
                }
            }

            return allSuccess;
        }

        #endregion

        #region Image Processing Operations

        public async Task<string> ResizeImageAsync(string imagePath, int width, int height, string folder = "thumbnails")
        {
            try
            {
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", imagePath.TrimStart('/'));

                if (!File.Exists(physicalPath))
                {
                    throw new FileNotFoundException("Şəkil tapılmadı");
                }

                using (var originalImage = Image.FromFile(physicalPath))
                {
                    using (var resizedImage = new Bitmap(width, height))
                    {
                        using (var graphics = Graphics.FromImage(resizedImage))
                        {
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            graphics.DrawImage(originalImage, 0, 0, width, height);
                        }

                        var uploadPath = Path.Combine(_baseUploadPath, folder);
                        EnsureDirectoryExists(uploadPath);

                        var fileName = GenerateUniqueFileName(Path.GetFileName(imagePath));
                        var newFilePath = Path.Combine(uploadPath, fileName);

                        // Save in same format as original
                        var format = GetImageFormat(Path.GetExtension(imagePath));
                        resizedImage.Save(newFilePath, format);

                        _logger.LogInformation("Image resized successfully: {OriginalPath} -> {NewPath}", imagePath, fileName);
                        return $"/uploads/{folder}/{fileName}";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resizing image: {ImagePath}", imagePath);
                throw;
            }
        }

        public async Task<string> CreateThumbnailAsync(string imagePath, int size = 150)
        {
            return await ResizeImageAsync(imagePath, size, size, "thumbnails");
        }

        public async Task<List<string>> CreateMultipleThumbnailsAsync(IEnumerable<string> imagePaths, int size = 150)
        {
            var thumbnails = new List<string>();

            foreach (var imagePath in imagePaths)
            {
                try
                {
                    var thumbnail = await CreateThumbnailAsync(imagePath, size);
                    thumbnails.Add(thumbnail);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating thumbnail for: {ImagePath}", imagePath);
                    // Continue with other images
                }
            }

            return thumbnails;
        }

        #endregion

        #region File Validation Operations

        public bool ValidateImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Şəkil faylı boş və ya mövcud deyil");
            }

            if (file.Length > _maxFileSize)
            {
                throw new ArgumentException($"Şəkil ölçüsü {_maxFileSize / (1024 * 1024)}MB-dan böyük ola bilməz");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
            {
                throw new ArgumentException($"Dəstəklənməyən şəkil formatı. İcazə verilən: {string.Join(", ", _allowedImageExtensions)}");
            }

            // Additional MIME type validation
            var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                throw new ArgumentException("Yanlış MIME type");
            }

            return true;
        }

        public bool ValidateDocumentFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Sənəd faylı boş və ya mövcud deyil");
            }

            if (file.Length > _maxFileSize)
            {
                throw new ArgumentException($"Sənəd ölçüsü {_maxFileSize / (1024 * 1024)}MB-dan böyük ola bilməz");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedDocumentExtensions.Contains(extension))
            {
                throw new ArgumentException($"Dəstəklənməyən sənəd formatı. İcazə verilən: {string.Join(", ", _allowedDocumentExtensions)}");
            }

            return true;
        }

        public bool IsImageFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedImageExtensions.Contains(extension);
        }

        public bool IsDocumentFile(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            return _allowedDocumentExtensions.Contains(extension);
        }

        #endregion

        #region File Information Operations

        public async Task<FileInfo> GetFileInfoAsync(string filePath)
        {
            try
            {
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));

                if (!File.Exists(physicalPath))
                {
                    return null;
                }

                var fileInfo = new FileInfo(physicalPath);
                return fileInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file info: {FilePath}", filePath);
                return null;
            }
        }

        public async Task<bool> FileExistsAsync(string filePath)
        {
            try
            {
                var physicalPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
                return File.Exists(physicalPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<long> GetFileSizeAsync(string filePath)
        {
            try
            {
                var fileInfo = await GetFileInfoAsync(filePath);
                return fileInfo?.Length ?? 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting file size: {FilePath}", filePath);
                return 0;
            }
        }

        public string GetFileExtension(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant();
        }

        public string GetMimeType(string fileName)
        {
            var extension = GetFileExtension(fileName);

            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".txt" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        #endregion

        #region Directory Operations

        public async Task<bool> CreateDirectoryAsync(string path)
        {
            try
            {
                var fullPath = Path.Combine(_baseUploadPath, path);

                if (!Directory.Exists(fullPath))
                {
                    Directory.CreateDirectory(fullPath);
                    _logger.LogInformation("Directory created: {Path}", fullPath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating directory: {Path}", path);
                return false;
            }
        }

        public async Task<IEnumerable<string>> GetDirectoryFilesAsync(string directoryPath)
        {
            try
            {
                var fullPath = Path.Combine(_baseUploadPath, directoryPath);

                if (!Directory.Exists(fullPath))
                {
                    return Enumerable.Empty<string>();
                }

                var files = Directory.GetFiles(fullPath)
                                   .Select(f => $"/uploads/{directoryPath}/{Path.GetFileName(f)}")
                                   .ToList();

                return files;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting directory files: {DirectoryPath}", directoryPath);
                return Enumerable.Empty<string>();
            }
        }

        public async Task<bool> DeleteDirectoryAsync(string directoryPath)
        {
            try
            {
                var fullPath = Path.Combine(_baseUploadPath, directoryPath);

                if (Directory.Exists(fullPath))
                {
                    Directory.Delete(fullPath, true);
                    _logger.LogInformation("Directory deleted: {Path}", fullPath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting directory: {DirectoryPath}", directoryPath);
                return false;
            }
        }

        #endregion

        #region Helper Methods

        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private string GenerateUniqueFileName(string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            var uniqueName = $"{fileNameWithoutExtension}_{Guid.NewGuid():N}{extension}";

            return uniqueName;
        }

        private ImageFormat GetImageFormat(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".png" => ImageFormat.Png,
                ".gif" => ImageFormat.Gif,
                ".bmp" => ImageFormat.Bmp,
                _ => ImageFormat.Jpeg
            };
        }

        #endregion
    }
}*/