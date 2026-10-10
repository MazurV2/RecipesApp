using Microsoft.EntityFrameworkCore;
using RecipesApi.DTOs.Recipe;
using RecipesApi.DTOs.RecipeIngredient;
using RecipesApi.DTOs.Step;
using RecipesApi.Entities;
using RecipesApi.Pagination;
using RecipesApi.Services.Interfaces;

namespace RecipesApi.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly AppDbContext _context;

        private readonly IFileService _fileService;
        private const string _recipeImagesFolder = "images/recipes";

        public RecipeService(AppDbContext context, IFileService fileService)
        {
            _context = context;
            _fileService = fileService;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<PagedResults<RecipeDTO>> GetRecipesAsync(RecipeQueryDTO queryDTO)
        {
            // Stwórz bazowe zapytanie do bazy danych
            var query = _context.Recipes.AsQueryable();

            query = ApplyFilters(query, queryDTO);
            query = ApplySorting(query, queryDTO);

            // Pobierz całkowitą liczbę przepisów po zastosowaniu filtrów
            var totalCount = await query.CountAsync();

            // Zastosuj paginację i przekształć wyniki na DTO
            var recipes = await query
                .Skip(queryDTO.PageSize * (queryDTO.PageNumber - 1))
                .Take(queryDTO.PageSize)
                .Select(r => new RecipeDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    RecipeIngredients = r.RecipeIngredients
                        .Select(ri => new RecipeIngredientDTO
                        {
                            IngredientId = ri.IngredientId,
                            IngredientName = ri.Ingredient.Name,
                            Amount = ri.Amount,
                            Unit = ri.Unit
                        }).ToList(),
                    Steps = r.Steps
                        .Select(s => new StepDTO
                        {
                            Id = s.Id,
                            StepNumber = s.StepNumber,
                            Description = s.Description,
                        }).ToList(),
                    Calories = r.Calories,
                    Difficulty = r.Difficulty.ToString()
                })
                .ToListAsync();

            var results = new PagedResults<RecipeDTO>(recipes, totalCount, queryDTO.PageNumber, queryDTO.PageSize);
            return results;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<RecipeDTO> GetRecipeByIdAsync(int id)
        {
            var recipe = await GetRecipeDtoById(id);

            if (recipe == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono przepisu o ID {id}");
            }

            return recipe;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<RecipeDTO> CreateRecipeAsync(CreateRecipeDTO createRecipeDTO, int userId)
        {
            // Sprawdź czy wprowadzone składniki istnieją
            var ingredientIds = createRecipeDTO.RecipeIngredients.Select(ri => ri.IngredientId).ToList();
            await CheckForMissingIngredients(ingredientIds);

            // Zapisz obraz przepisu, jeśli został przesłany
            string? imageUrl = await SaveImageGetUrl(createRecipeDTO.Image);

            // Utwórz nowy przepis na podstawie danych z DTO
            var recipe = new Recipe
            {
                UserId = userId,
                Title = createRecipeDTO.Title,
                Description = createRecipeDTO.Description,
                ImageUrl = imageUrl,
                RecipeIngredients = createRecipeDTO.RecipeIngredients
                    .Select(ri => new RecipeIngredient
                    {
                        IngredientId = ri.IngredientId,
                        Amount = ri.Amount,
                        Unit = ri.Unit
                    }).ToList(),
                Steps = createRecipeDTO.Steps
                    .Select(s => new Step
                    {
                        Description = s.Description
                    }).ToList(),
                Calories = createRecipeDTO.Calories,
                Difficulty = createRecipeDTO.Difficulty,
            };

            // Dodaj nowy przepis do bazy danych
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();

            var recipeDTO = await GetRecipeDtoById(recipe.Id);
            return recipeDTO;
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task<RecipeDTO> UpdateRecipeAsync(int id, UpdateRecipeDTO updateRecipeDTO, int userId)
        {
            var recipe = await _context.Recipes
                .Include(r => r.RecipeIngredients)
                .Include(r => r.Steps)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono przepisu o ID {id}");
            }

            // Sprawdź czy przepis należy do zalogowanego użytkownika
            if (recipe.UserId != userId)
            {
                throw new UnauthorizedAccessException("Nie masz uprawnień do modyfikacji tego przepisu");
            }

            // Sprawdź czy wprowadzone składniki istnieją
            var ingredientIds = updateRecipeDTO.RecipeIngredients.Select(ri => ri.IngredientId).ToList();
            await CheckForMissingIngredients(ingredientIds);

            // Zapisz nowy i usuń stary obraz, jeśli został przesłany
            string? imageUrl = await SaveImageGetUrl(updateRecipeDTO.Image);
            if (imageUrl != null && !string.IsNullOrWhiteSpace(recipe.ImageUrl))
            {
                _fileService.DeleteFile(recipe.ImageUrl);
            }
                        
            recipe.Title = updateRecipeDTO.Title;
            recipe.Description = updateRecipeDTO.Description;
            recipe.ImageUrl = imageUrl ?? recipe.ImageUrl;
            recipe.Calories = updateRecipeDTO.Calories;
            recipe.Difficulty = updateRecipeDTO.Difficulty;

            // Zaktualizuj składniki przepisu
            UpdateRecipeIngredients(recipe, updateRecipeDTO.RecipeIngredients);

            // Zaktualizuj kroki przepisu
            UpdateRecipeSteps(recipe, updateRecipeDTO.Steps);

            // Zapisz zmiany w bazie danych
            await _context.SaveChangesAsync();

            var recipeDTO = await GetRecipeDtoById(id);
            return recipeDTO;
        }

        private void UpdateRecipeIngredients(Recipe recipe, ICollection<UpdateRecipeIngredientDTO> recipeIngredients)
        {
            // Usuń składniki, których nie ma w nowej liście
            var newIngredientIds = recipeIngredients.Select(ri => ri.IngredientId).ToList();
            var ingredientsToRemove = recipe.RecipeIngredients.Where(ri => !newIngredientIds.Contains(ri.IngredientId)).ToList();

            foreach (var ri in ingredientsToRemove)
            {
                recipe.RecipeIngredients.Remove(ri);
            }

            // Zaktualizuj lub dodaj składniki przepisu
            foreach (var ri in recipeIngredients)
            {
                // Pobierz składnik jeśli istnieje
                var existingIngredient = recipe.RecipeIngredients.FirstOrDefault(r => r.IngredientId == ri.IngredientId);

                if (existingIngredient != null) // Zaktualizuj istniejący składnik
                {
                    existingIngredient.Amount = ri.Amount;
                    existingIngredient.Unit = ri.Unit;
                }
                else // Dodaj nowy składnik
                {
                    recipe.RecipeIngredients.Add(new RecipeIngredient
                    {
                        IngredientId = ri.IngredientId,
                        Amount = ri.Amount,
                        Unit = ri.Unit
                    });
                }
            }
        }

        private void UpdateRecipeSteps(Recipe recipe, ICollection<UpdateStepDTO> steps)
        {
            // Usuń kroki, których nie ma w nowej liście
            var newStepIds = steps
                .Where(s => s.Id.HasValue && s.Id.Value > 0)
                .Select(s => s.Id!.Value)
                .ToList();
            var stepsToRemove = recipe.Steps.Where(s => !newStepIds.Contains(s.Id)).ToList();
            
            foreach (var step in stepsToRemove)
            {
                recipe.Steps.Remove(step);
            }

            // Zaktualizuj lub dodaj kroki przepisu
            for (int i = 0; i < steps.Count; i++)
            {
                var step = steps.ElementAt(i);
                var stepNumber = i + 1; // Ustal numer kroku na bazie pozycji w liście

                Step? existingStep = null;
                if (step.Id.HasValue && step.Id.Value > 0)
                {
                    // Pobierz krok jeśli istnieje
                    existingStep = recipe.Steps.FirstOrDefault(s => s.Id == step.Id.Value);
                }

                if (existingStep != null) // Zaktualizuj istniejący krok
                {
                    existingStep.StepNumber = stepNumber;
                    existingStep.Description = step.Description;
                }
                else // Dodaj nowy krok
                {
                    recipe.Steps.Add(new Step
                    {
                        StepNumber = stepNumber,
                        Description = step.Description
                    });
                }
            }
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        public async Task DeleteRecipeAsync(int id, int userId)
        {
            var recipe = await _context.Recipes
                .FirstOrDefaultAsync(r => r.Id == id);

            if (recipe == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono przepisu o ID {id}");
            }

            // Sprawdź czy przepis należy do zalogowanego użytkownika
            if (recipe.UserId != userId)
            {
                throw new UnauthorizedAccessException("Nie masz uprawnień do usunięcia tego przepisu");
            }

            // Usuń obraz przepisu, jeśli istnieje
            if (!string.IsNullOrWhiteSpace(recipe.ImageUrl))
            {
                _fileService.DeleteFile(recipe.ImageUrl);
            }

            // Usuń przepis
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }

        // ----- ----- ----- ----- ----- ----- ----- ----- ----- ----- -----

        private async Task<RecipeDTO> GetRecipeDtoById(int id)
        {
            var recipeDto = await _context.Recipes
                .Where(r => r.Id == id)
                .Select(r => new RecipeDTO
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    RecipeIngredients = r.RecipeIngredients
                        .Select(ri => new RecipeIngredientDTO
                        {
                            IngredientId = ri.IngredientId,
                            IngredientName = ri.Ingredient.Name,
                            Amount = ri.Amount,
                            Unit = ri.Unit
                        }).ToList(),
                    Steps = r.Steps
                        .Select(s => new StepDTO
                        {
                            Id = s.Id,
                            StepNumber = s.StepNumber,
                            Description = s.Description,
                        }).ToList(),
                    Calories = r.Calories,
                    Difficulty = r.Difficulty.ToString()
                })
                .FirstOrDefaultAsync();

            if (recipeDto == null)
            {
                throw new KeyNotFoundException($"Nie znaleziono przepisu o ID {id}");
            }

            return recipeDto;
        }

        private async Task<string?> SaveImageGetUrl(IFormFile? image)
        {
            if (image != null && image.Length > 0)
            {
                var imageUrl = await _fileService.SaveFileAsync(image, _recipeImagesFolder);
                return imageUrl;
            }
            return null;
        }

        private async Task CheckForMissingIngredients(List<int> ingredientIds)
        {
            // Sprawdź czy podane id składników istnieją w bazie
            var existingIngredientIds = await _context.Ingredients
                .Where(i => ingredientIds.Contains(i.Id))
                .Select(i => i.Id)
                .ToListAsync();

            // Zbierz nieistniejące składniki
            var missingIngredientIds = ingredientIds.Except(existingIngredientIds).ToList();

            if (missingIngredientIds.Any())
            {
                throw new KeyNotFoundException($"Następujące ID składników nie istnieją: {string.Join(", ", missingIngredientIds)}");
            }
        }

        private IQueryable<Recipe> ApplyFilters(IQueryable<Recipe> query, RecipeQueryDTO queryDTO)
        {
            // Filtruj po szukanej frazie
            if (!string.IsNullOrWhiteSpace(queryDTO.SearchTerm))
            {
                var searchTerm = queryDTO.SearchTerm.ToLower();
                query = query.Where(r =>
                    r.Title.ToLower().Contains(searchTerm) ||
                    r.Description.ToLower().Contains(searchTerm)
                );
            }
            // Filtruj po kaloriach
            if (queryDTO.MinCalories.HasValue)
            {
                query = query.Where(r => r.Calories >= queryDTO.MinCalories.Value);
            }

            if (queryDTO.MaxCalories.HasValue)
            {
                query = query.Where(r => r.Calories <= queryDTO.MaxCalories.Value);
            }
            // Filtruj po poziomie trudności
            if (queryDTO.MinDifficulty.HasValue)
            {
                query = query.Where(r => (int)r.Difficulty >= queryDTO.MinDifficulty.Value);
            }
            if (queryDTO.MaxDifficulty.HasValue)
            {
                query = query.Where(r => (int)r.Difficulty <= queryDTO.MaxDifficulty.Value);
            }
            return query;
        }

        private IQueryable<Recipe> ApplySorting(IQueryable<Recipe> query, RecipeQueryDTO queryDTO)
        {
            // Sortuj po wybranym polu i kierunku
            query = queryDTO.SortBy?.ToLower() switch
            {
                "title" => queryDTO.SortDescending ? query.OrderByDescending(r => r.Title) : query.OrderBy(r => r.Title),
                "calories" => queryDTO.SortDescending ? query.OrderByDescending(r => r.Calories) : query.OrderBy(r => r.Calories),
                "difficulty" => queryDTO.SortDescending ? query.OrderByDescending(r => r.Difficulty) : query.OrderBy(r => r.Difficulty),
                _ => queryDTO.SortDescending ? query.OrderByDescending(r => r.Id) : query.OrderBy(r => r.Id),
            };
            return query;
        }
    }
}
