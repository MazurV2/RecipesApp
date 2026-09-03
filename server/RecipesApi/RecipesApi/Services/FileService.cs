using RecipesApi.Services.Interfaces;

namespace RecipesApi.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string filePath)
        {
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, filePath);

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = GetUniQueFileName(file.FileName);

            var finalFilePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(finalFilePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
                fileStream.Close();
            }

            return finalFilePath;
        }

        public bool DeleteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return false;

            var fullFilePath = Path.Combine(GetWebRootPath(), filePath);
            
            if (File.Exists(fullFilePath))
            {
                File.Delete(fullFilePath);
                return true;
            }

            return false;
        }

        private string GetWebRootPath()
        {
            return _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        private string GetUniQueFileName(string fileName)
        {
            var fileExtension = Path.GetExtension(fileName);
            return $"{Guid.NewGuid()}{fileExtension}";
        }
    }
}
