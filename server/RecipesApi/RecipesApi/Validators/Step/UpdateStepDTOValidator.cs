using FluentValidation;
using RecipesApi.DTOs.Step;

namespace RecipesApi.Validators.Step
{
    public class UpdateStepDTOValidator : AbstractValidator<UpdateStepDTO>
    {
        private int maxDescriptionLength = 200;

        public UpdateStepDTOValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Należy podać treść kroku instrukcji.")
                .MaximumLength(maxDescriptionLength).WithMessage($"Opis kroku nie może przekraczać {maxDescriptionLength} znaków.");
        }
    }
}
