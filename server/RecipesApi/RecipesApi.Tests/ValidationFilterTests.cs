using RecipesApi.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Abstractions;
using RecipesApi.DTOs.Recipe;
using FluentValidation.Results;
using FluentValidation;
using NSubstitute;


namespace RecipesApi.Tests
{
    public class ValidationFilterTests
    {
        [Fact]
        public async Task OnActionExecutionAsync_FluentValidationFail_ReturnsBadRequest()
        {
            // 1. Arrange
            var filter = new ValidationFilter();

            // Stwórz przykładowy DTO z nieprawidłowymi danymi
            var testDto = new CreateRecipeDTO { Title = "" };

            // Stwórz wynik walidacji z błędami
            var failures = new List<ValidationFailure>
            {
                new ValidationFailure("Title", "Tytuł jest wymagany.")
            };
            var validationResult = new ValidationResult(failures);

            // Zmockuj walidator i jego zachowanie
            var validatorMock = Substitute.For<IValidator>();
            validatorMock.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
                .Returns(validationResult);

            // Zmockuj IServiceProvider, aby zwracał nasz mock walidatora
            var serviceProviderMock = Substitute.For<IServiceProvider>();
            serviceProviderMock.GetService(Arg.Any<Type>())
                .Returns(validatorMock);

            // Stwórz kontekst akcji z mockiem IServiceProvider
            var httpContext = new DefaultHttpContext
            {
                RequestServices = serviceProviderMock
            };

            // Stwórz ActionExecutingContext z naszym DTO
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            var actionExecutingContext = new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object> { { "dto", testDto } },
                new object()
            );

            // Ustaw flagę, zależnie od tego, czy next() został wywołany
            bool nextExecuted = false;
            ActionExecutionDelegate next = () =>
            {
                nextExecuted = true;
                return Task.FromResult<ActionExecutedContext>(null);
            };

            // 2. Act
            await filter.OnActionExecutionAsync(actionExecutingContext, next);

            // 3. Assert
            // Sprawdź, czy potok został przerwany
            Assert.False(nextExecuted);

            // Sprawdź, czy zwrócono odpowiedź BadRequest
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionExecutingContext.Result);
            Assert.Equal(400, badRequestResult.StatusCode);
        }
    }
}
