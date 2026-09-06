
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using RecipesApi.Services;
using System.Text;

namespace RecipesApi.Tests
{
    public class FileServiceTests
    {
        private readonly IWebHostEnvironment _webHostEnvinmentMock;
        private readonly FileService _fileService;
        private readonly string _testWebRootPath;
        
        public FileServiceTests()
        {
            _testWebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot_test");
            
            _webHostEnvinmentMock = Substitute.For<IWebHostEnvironment>();
            _webHostEnvinmentMock.WebRootPath.Returns(_testWebRootPath);
            
            _fileService = new FileService(_webHostEnvinmentMock);
        }

        private IFormFile CreateFakeFormFile(string fileName, string content)
        {
            // Tworzenie fałszywego pliku IFormFile
            var fileMock = Substitute.For<IFormFile>();
            fileMock.FileName.Returns(fileName);

            // Tworzenie strumienia z zawartością pliku
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            fileMock.OpenReadStream().Returns(stream);
            
            return fileMock;
        }

        [Fact]
        public async Task SaveFileAsync_ValidFile_ShouldSaveAndReturnPath()
        {
            // Arrange
            var fileMock = CreateFakeFormFile("testfile.txt", "Plik testowy.");

            // Określ ścieżkę zapisu pliku
            var filePath = "uploads";
            var fileExtension = Path.GetExtension(filePath);
            
            // Act
            var result = await _fileService.SaveFileAsync(fileMock, filePath);
            
            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));
            Assert.EndsWith(fileExtension, result);
            
            // Clean up
            File.Delete(result);
        }

        [Fact]
        public async Task SaveFileAsync_DirectoryDoesNotExist_ShouldCreateDirectoryAndSaveFile()
        {
            // Arrange
            var fileMock = CreateFakeFormFile("testfile.txt", "Plik testowy.");

            var filePath = "uploads/newfolder";

            // Act
            var result = await _fileService.SaveFileAsync(fileMock, filePath);

            // Assert
            Assert.NotNull(result);
            Assert.True(File.Exists(result));

            // Clean up
            File.Delete(result);
        }

        [Fact]
        public async Task SaveFileAsync_FileIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var filePath = "uploads";
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _fileService.SaveFileAsync(null!, filePath));
        }

        [Fact]
        public async Task SaveFileAsync_FilePathIsNullOrEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            var fileMock = CreateFakeFormFile("testfile.txt", "Plik testowy.");
            
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _fileService.SaveFileAsync(fileMock, null!));
            await Assert.ThrowsAsync<ArgumentNullException>(() => _fileService.SaveFileAsync(fileMock, string.Empty));
        }

        [Fact]
        public void DeleteFile_ExistingFile_ShouldDeleteAndReturnTrue()
        {
            // Arrange


            // Stwórz tymczasowy plik do usunięcia
            Directory.CreateDirectory(_testWebRootPath);
            var tempFilePath = Path.Combine(_testWebRootPath, "testfile.txt");
            File.WriteAllText(tempFilePath, "Testowy plik tymczasowy.");
            
            // Act
            var result = _fileService.DeleteFile("testfile.txt");
            
            // Assert
            Assert.True(result);
            Assert.False(File.Exists(tempFilePath));
        }

        [Fact]
        public void DeleteFile_NonExistingFile_ShouldReturnFalse()
        {
            // Arrange

            // Act
            var result = _fileService.DeleteFile("nonexistentfile.txt");

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void DeleteFile_InvalidFilePath_ShouldReturnFalse(string? filePath)
        {
            // Arrange
                        
            // Act
            var result = _fileService.DeleteFile(filePath);
            
            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            // Clean up the test directory after tests
            if (Directory.Exists(_testWebRootPath))
            {
                Directory.Delete(_testWebRootPath, true);
            }
        }
    }
}
