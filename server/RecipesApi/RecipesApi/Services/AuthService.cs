using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Auth;
using RecipesApi.DTOs.User;
using RecipesApi.Entities;
using RecipesApi.Services.Interfaces;

namespace RecipesApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext context, ITokenService tokenService, IValidator<RegisterDTO> registerValidator, IValidator<LoginDTO> loginValidator)
        {
            _context = context;
            _tokenService = tokenService;
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

            // Wygenerowanie tokenu JWT
            var (token, expiryDate) = _tokenService.GenerateJwtToken(user);

            var response = new AuthResponseDTO
            {
                Token = token,
                ExpirationDate = expiryDate
            };

            return ServiceResult<AuthResponseDTO>.Success(response);
        }

        
    }
}
