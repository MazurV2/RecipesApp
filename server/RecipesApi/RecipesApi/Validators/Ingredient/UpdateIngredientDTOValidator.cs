using FluentValidation;
using RecipesApi.DTOs.Ingredient;

namespace RecipesApi.Validators.Ingredient
{
    public class UpdateIngredientDTOValidator : AbstractValidator<UpdateIngredientDTO>
    {
        private int maxNameLength = 50;

        public UpdateIngredientDTOValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Nazwa składnika jest wymagana.")
                .MaximumLength(maxNameLength).WithMessage($"Nazwa składnika nie może przekraczać {maxNameLength} znaków.");
        }
    }
}
