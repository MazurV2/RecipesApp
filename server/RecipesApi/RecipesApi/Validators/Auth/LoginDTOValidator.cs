using FluentValidation;
using RecipesApi.DTOs.Auth;

namespace RecipesApi.Validators.Auth
{
    public class LoginDTOValidator : AbstractValidator<LoginDTO>
    {
        public LoginDTOValidator()
        {
            RuleFor(x => x.UsernameOrEmail)
                .NotEmpty().WithMessage("Nazwa użytkownika lub email jest wymagany.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Hasło jest wymagane.");
        }
    }
}
