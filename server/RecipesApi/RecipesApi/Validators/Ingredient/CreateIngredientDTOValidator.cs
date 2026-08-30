using FluentValidation;
using RecipesApi.DTOs.Ingredient;

namespace RecipesApi.Validators.Ingredient
{
    public class CreateIngredientDTOValidator : AbstractValidator<CreateIngredientDTO>
    {
        private int maxNameLength = 50;

        public CreateIngredientDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nazwa składnika jest wymagana.")
                .MaximumLength(maxNameLength).WithMessage($"Nazwa składnika nie może przekraczać {maxNameLength} znaków.");
        }
    }
}
