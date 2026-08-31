using FluentValidation;
using RecipesApi.DTOs.Step;

namespace RecipesApi.Validators.Step
{
    public class CreateStepDTOValidator : AbstractValidator<CreateStepDTO>
    {
        private int maxDescriptionLength = 200;

        public CreateStepDTOValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Należy podać treść kroku instrukcji.")
                .MaximumLength(maxDescriptionLength).WithMessage($"Opis kroku nie może przekraczać {maxDescriptionLength} znaków.");
        }
    }
}
