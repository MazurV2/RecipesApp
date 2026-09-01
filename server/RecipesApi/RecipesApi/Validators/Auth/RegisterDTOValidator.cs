using FluentValidation;
using RecipesApi.DTOs.Auth;

namespace RecipesApi.Validators.Auth
{
    public class RegisterDTOValidator : AbstractValidator<RegisterDTO>
    {
        private int minUsernameLength = 3;
        private int maxUsernameLength = 30;
        private int minPasswordLength = 8;
        private int maxPasswordLength = 30;

        public RegisterDTOValidator()
        {
            RuleFor(x => x.Username)
                .NotEmpty().WithMessage("Nazwa użytkownika jest wymagana.")
                .MinimumLength(minUsernameLength).WithMessage($"Nazwa użytkownika musi składać się przynajmniej z {minUsernameLength} znaków.")
                .MaximumLength(maxUsernameLength).WithMessage($"Nazwa użytkownika musi składać się co najwyżej z {maxUsernameLength} znaków.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Adres email jest wymagany.")
                .EmailAddress().WithMessage("Wprowadź poprawny adres email.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Hasło jest wymagane.")
                .MinimumLength(minPasswordLength).WithMessage($"Hasło musi składać się przynajmniej z {minPasswordLength} znaków.")
                .MaximumLength(maxPasswordLength).WithMessage($"Hasło musi składać się co najwyżej z {maxPasswordLength} znaków.")
                .Matches(@"[a-z]+").WithMessage("Hasło musi zawierać przynajmniej jedną małą literę.")
                .Matches(@"[A-Z]+").WithMessage("Hasło musi zawierać przynajmniej jedną wielką literę.")
                .Matches(@"\d+").WithMessage("Hasło musi zawierać przynajmniej jedną cyfrę.")
                .Matches(@"[!@#$%^&*()_+{}\[\]:;<>,.?~\\/-]+").WithMessage("Hasło musi zawierać przynajmniej jeden znak specjalny.")
                .Equal(x => x.PasswordConfirmation).WithMessage("Hasła muszą być zgodne.");
        }
    }
}
