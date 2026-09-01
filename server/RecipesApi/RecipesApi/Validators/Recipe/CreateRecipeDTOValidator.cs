using FluentValidation;
using RecipesApi.DTOs.Recipe;
using RecipesApi.Validators.RecipeIngredient;
using RecipesApi.Validators.Step;

namespace RecipesApi.Validators.Recipe
{
    public class CreateRecipeDTOValidator : AbstractValidator<CreateRecipeDTO>
    {
        private int maxTitleLength = 100;
        private int maxDescriptionLength = 300;
        private int maxCalories = 100000;

        public CreateRecipeDTOValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Tytuł jest wymagany.")
                .MaximumLength(maxTitleLength).WithMessage($"Tytuł nie może przekraczać {maxTitleLength} znaków.");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Opis jest wymagany.")
                .MaximumLength(maxDescriptionLength).WithMessage($"Opis nie może przekraczać {maxDescriptionLength} znaków.");

            RuleFor(x => x.Image)
                .Must(CheckForValidImage).WithMessage("Przesłany plik nie jest poprawnym formatem obrazu.")
                .Must(CheckForImageSize).WithMessage("Przesłany obraz przekracza maksymalny dozwolony rozmiar.")
                .When(x => x.Image != null);

            RuleFor(x => x.RecipeIngredients)
                .NotEmpty().WithMessage("Przepis musi zawierać co najmniej jeden składnik.");

            RuleFor(x => x.Steps)
                .NotEmpty().WithMessage("Przepis musi zawierać co najmniej jeden krok.");

            RuleFor(x => x.Calories)
                .InclusiveBetween(0, maxCalories).WithMessage($"Liczba kalorii musi mieścić się w zakresie 0-{maxCalories}.");
            
            RuleFor(x => x.Difficulty)
                .IsInEnum().WithMessage("Wybierz poprawną trudność wykonania przepisu.");

            // Ustawienie walidatorów dla elementów kolekcji
            RuleForEach(x => x.RecipeIngredients).SetValidator(new CreateRecipeIngredientDTOValidator());
            RuleForEach(x => x.Steps).SetValidator(new CreateStepDTOValidator());
        }

        private bool CheckForValidImage(IFormFile? image)
        {
            // Brak pliku jest dozwolony, więc zwracamy true
            if (image == null) return true;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var imageExtenstion = Path.GetExtension(image.FileName).ToLowerInvariant();
            
            return allowedExtensions.Contains(imageExtenstion);
        }

        private bool CheckForImageSize(IFormFile? image)
        {
            // Brak pliku jest dozwolony, więc zwracamy true
            if (image == null) return true;

            var maxImageSize = 8 * 1024 * 1024; // 8 MB
            var imageSize = image.Length;

            return imageSize <= maxImageSize;
        }
    }
}
