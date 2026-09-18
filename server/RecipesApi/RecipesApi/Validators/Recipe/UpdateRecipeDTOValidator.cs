using FluentValidation;
using RecipesApi.DTOs.Recipe;
using RecipesApi.Validators.RecipeIngredient;
using RecipesApi.Validators.Step;
namespace RecipesApi.Validators.Recipe
{
    public class UpdateRecipeDTOValidator : AbstractValidator<UpdateRecipeDTO>
    {
        public UpdateRecipeDTOValidator()
        {
            Include(new BaseRecipeDTOValidator<UpdateRecipeDTO>());

            RuleFor(x => x.RecipeIngredients)
                .NotEmpty().WithMessage("Przepis musi zawierać co najmniej jeden składnik.");

            RuleFor(x => x.Steps)
                .NotEmpty().WithMessage("Przepis musi zawierać co najmniej jeden krok.");

            // Ustawienie walidatorów dla elementów kolekcji
            RuleForEach(x => x.RecipeIngredients).SetValidator(new UpdateRecipeIngredientDTOValidator());
            RuleForEach(x => x.Steps).SetValidator(new UpdateStepDTOValidator());
        }
    }
}
