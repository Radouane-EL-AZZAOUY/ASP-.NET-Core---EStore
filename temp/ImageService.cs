using System.IO;

namespace TP1.Services
{
    public class ImageService
    {
        private readonly IWebHostEnvironment _environment;
        private const string ImagesFolder = "images/movies";
        private const string GalleryFolder = "images/movies/gallery";

        public ImageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SaveImageAsync(IFormFile? file, int movieId, bool isMainImage = false)
        {
            if (file == null || file.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return null;

            // Create directory if it doesn't exist
            var folder = isMainImage ? ImagesFolder : GalleryFolder;
            var uploadPath = Path.Combine(_environment.WebRootPath, folder);
            Directory.CreateDirectory(uploadPath);

            // Generate unique filename
            var fileName = $"{movieId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative path
            return $"/{folder}/{fileName}";
        }

        public async Task<List<string>> SaveGalleryImagesAsync(List<IFormFile>? files, int movieId)
        {
            var savedPaths = new List<string>();

            if (files == null || !files.Any())
                return savedPaths;

            foreach (var file in files)
            {
                var path = await SaveImageAsync(file, movieId, isMainImage: false);
                if (path != null)
                {
                    savedPaths.Add(path);
                }
            }

            return savedPaths;
        }

        public void DeleteImage(string? imagePath)
        {
            if (string.IsNullOrEmpty(imagePath))
                return;

            var filePath = Path.Combine(_environment.WebRootPath, imagePath.TrimStart('/'));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public string GetDefaultImagePath()
        {
            return "/images/movies/default.jpg";
        }
    }
}

