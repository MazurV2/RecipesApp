using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RecipesApi.DTOs.Auth;
using RecipesApi.Services;
using RecipesApi.Services.Interfaces;
using RecipesApi.Settings;

namespace RecipesApi.Tests
{
    public class AuthServiceTests : IDisposable
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new AppDbContext(options);

            var jwtSettings = new JwtSettings
            {
                Secret = "TestSecretKeyTestSecretKeyTestSecretKeyTestSecretKey",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpiryInMinutes = 5
            };

            var tokenServiceOptions = Options.Create(jwtSettings);

            _tokenService = new TokenService(tokenServiceOptions);
            _authService = new AuthService(_context, _tokenService);
        }

        [Fact]
        public async Task Register_ValidRegisterData_ShouldReturnSuccess()
        {
            // Arrange
            var registerDTO = new RegisterDTO
            {
                Username = "test",
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Act
            var result = await _authService.Register(registerDTO);

            // Assert
            // Sprawdź wynik rejestracji
            Assert.True(result.IsSuccess);
            
            // Sprawdź, czy dane użytkownika są poprawne
            Assert.NotNull(result.Data);
            Assert.Equal(registerDTO.Username, result.Data.Username);
            Assert.Equal(registerDTO.Email, result.Data.Email);

            // Sprawdź, czy użytkownik został dodany do bazy danych
            var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.Username == registerDTO.Username);
            Assert.NotNull(userInDb);
        }

        [Fact]
        public async Task Register_NameExists_ShouldReturnFailure()
        {
            // Arrange
            var registerDTO = new RegisterDTO
            {
                Username = "test",
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Dodaj użytkownika do bazy danych, aby sprawdzić przypadek, gdy nazwa lub email już istnieje
            _context.Users.Add(new Entities.User
            {
                Username = registerDTO.Username,
                Email = "existing@example.com",
                PasswordHash = "hashedpassword"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.Register(registerDTO);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(result.ResultMessage));
        }

        [Fact]
        public async Task Register_EmailExists_ShouldReturnFailure()
        {
            // Arrange
            var registerDTO = new RegisterDTO
            {
                Username = "test",
                Email = "test@example.com",
                Password = "TestPassword123!"
            };

            // Dodaj użytkownika do bazy danych, aby sprawdzić przypadek, gdy nazwa lub email już istnieje
            _context.Users.Add(new Entities.User
            {
                Username = "existing",
                Email = registerDTO.Email,
                PasswordHash = "hashedpassword"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.Register(registerDTO);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(result.ResultMessage));
        }

        [Fact]
        public async Task Login_ValidUsernameAndPassword_ShouldReturnSuccessWithToken()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                UsernameOrEmail = "test",
                Password = "TestPassword123!"
            };

            // Dodaj użytkownika do bazy danych
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(loginDTO.Password);
            _context.Users.Add(new Entities.User
            {
                Username = loginDTO.UsernameOrEmail,
                Email = loginDTO.UsernameOrEmail,
                PasswordHash = passwordHash
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.Login(loginDTO);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.False(string.IsNullOrWhiteSpace(result.Data.Token));
            Assert.True(result.Data.ExpiryDate > DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_ValidEmailAndPassword_ShouldReturnSuccessWithToken()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                UsernameOrEmail = "test@example.com",
                Password = "TestPassword123!"
            };

            // Dodaj użytkownika do bazy danych
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(loginDTO.Password);
            _context.Users.Add(new Entities.User
            {
                Username = "test",
                Email = loginDTO.UsernameOrEmail,
                PasswordHash = passwordHash
            }); 
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.Login(loginDTO);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.False(string.IsNullOrWhiteSpace(result.Data.Token));
            Assert.True(result.Data.ExpiryDate > DateTime.UtcNow);
        }

        [Fact]
        public async Task Login_InvalidUsernameOrEmail_ShouldReturnFailure()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                UsernameOrEmail = "incorrect",
                Password = "TestPassword123!"
            };

            // Dodaj użytkownika do bazy danych
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!");
            _context.Users.Add(new Entities.User
            {
                Username = "test",
                Email = "test@example.com",
                PasswordHash = passwordHash
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.Login(loginDTO);
            
            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(result.ResultMessage));
        }

        [Fact]
        public async Task Login_InvalidPassword_ShouldReturnFailure()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                UsernameOrEmail = "test",
                Password = "IncorrectPassword!"
            };

            // Dodaj użytkownika do bazy danych
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("TestPassword123!");
            _context.Users.Add(new Entities.User
            {
                Username = "test",
                Email = "test@example.com",
                PasswordHash = passwordHash
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _authService.Login(loginDTO);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(result.ResultMessage));
        }

        public void Dispose()
        {
            // Usuń bazę danych po zakończeniu testów
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
