using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using RecipesApi.DTOs.Auth;
using RecipesApi.DTOs.User;
using RecipesApi.Entities;
using RecipesApi.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RecipesApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly JwtSettings _jwtSettings;

        public AuthService(AppDbContext context, IOptions<JwtSettings> jwtSettings)
        {
            _context = context;
            _jwtSettings = jwtSettings.Value;
        }

        public async Task<ServiceResult<UserDTO>> Register(RegisterDTO registerDTO)
        {
            // Sprawdź, czy użytkownik o podanej nazwie lub emailu już istnieje
            if (await _context.Users.AnyAsync(u => u.Username == registerDTO.Username))
            {
                return ServiceResult<UserDTO>.Failure("Użytkownik o podanej nazwie już istnieje.");
            }

            if (await _context.Users.AnyAsync(u => u.Email == registerDTO.Email))
            {
                return ServiceResult<UserDTO>.Failure("Użytkownik o podanym emailu już istnieje.");
            }

            // Zaszyfruj hasło
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDTO.Password);

            // Utwórz nowego użytkownika
            var user = new User
            {
                Username = registerDTO.Username,
                Email = registerDTO.Email,
                PasswordHash = passwordHash
            };

            // Dodaj użytkownika do bazy danych
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Zwróć dane nowo utworzonego użytkownika
            var userDTO = new UserDTO
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            };

            return ServiceResult<UserDTO>.Success(userDTO);
        }
    
        public async Task<ServiceResult<AuthResponseDTO>> Login(LoginDTO loginDTO)
        {
            // Znajdź użytkownika na podstawie nazwy użytkownika lub adresu e-mail
            var user = await _context.Users.FirstOrDefaultAsync(
                u => u.Username == loginDTO.UsernameOrEmail || u.Email == loginDTO.UsernameOrEmail
                );

            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDTO.Password, user.PasswordHash))
            {
                return ServiceResult<AuthResponseDTO>.Failure("Wprowadzono nieprawidłowy login/email lub hasło.");
            }

            var jwtTokenWithExpiry = GenerateJwtToken(user);

            var response = new AuthResponseDTO
            {
                Token = jwtTokenWithExpiry.Item1,
                ExpirationDate = jwtTokenWithExpiry.Item2
            };

            return ServiceResult<AuthResponseDTO>.Success(response);
        }

        private (string, DateTime) GenerateJwtToken(User user)
        {
            // Określ informacje zawarte w tokenie
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Pobierz sekret i stwórz klucz szyfrujący
            var secretKey = _jwtSettings.Secret;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryDate = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryInMinutes);

            // Wygeneruj token (JWT)
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiryDate,
                signingCredentials: credentials
                );

            var writtenToken = new JwtSecurityTokenHandler().WriteToken(token);

            return (writtenToken, expiryDate);
        }
    }
}
