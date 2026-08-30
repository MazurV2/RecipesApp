using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RecipesApi.Filters
{
    public class ValidationFilter : IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            foreach (var actionArgument in context.ActionArguments.Values)
            {
                if (actionArgument == null) continue;

                // Pobierz typ DTO
                var dtoType = actionArgument.GetType();

                // Stwórz szablon walidatora i przekształć go w typ dla aktualnego DTO
                // np. IValidator<RegisterDTO>
                var validatorType = typeof(IValidator<>).MakeGenericType(dtoType);

                // Znajdź zarejesrowany walidator
                var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;
                
                if (validator != null)
                {
                    // Spakuj przesłane dane do formatu biblioteki FluentValidation
                    var validationContext = new ValidationContext<object>(actionArgument);

                    // Dokonaj walidacji na bazie konkretnego walidatora
                    var validationResult = await validator.ValidateAsync(validationContext);
                
                    if (!validationResult.IsValid)
                    {
                        context.Result = new BadRequestObjectResult(validationResult.ToDictionary());
                        return;
                    }
                }
            }

            await next();
        }
    }
}
