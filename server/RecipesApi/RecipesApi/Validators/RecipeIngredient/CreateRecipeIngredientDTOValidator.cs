using FluentValidation;
using RecipesApi.DTOs.RecipeIngredient;

namespace RecipesApi.Validators.RecipeIngredient
{
    public class CreateRecipeIngredientDTOValidator : AbstractValidator<CreateRecipeIngredientDTO>
    {
        private int maxUnitLength = 30;

        public CreateRecipeIngredientDTOValidator()
        {
            RuleFor(x => x.IngredientId)
                .NotEmpty().WithMessage("Należy wybrać składnik.");

            RuleFor(x => x.Amount)
                .NotEmpty().WithMessage("Należy podać ilość.")
                .GreaterThan(0).WithMessage("Ilość musi być większa od 0.");

            RuleFor(x => x.Unit)
                .NotEmpty().WithMessage("Należy podać jednostkę.")
                .MaximumLength(maxUnitLength).WithMessage($"Jednostka nie może przekraczać {maxUnitLength} znaków");
        }
    }
}
