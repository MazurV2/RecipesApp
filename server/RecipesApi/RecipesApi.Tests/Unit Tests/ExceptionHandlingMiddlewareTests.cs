
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RecipesApi.Middleware;
using System.Text.Json;
using Moq;

namespace RecipesApi.Tests.Unit_Tests
{
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock = new();

        // Test middleware do obsługi wyjątków
        [Fact]
        public async Task GetTestError_ShouldReturnInternalServerError()
        {
            // Arrange
            // Stwórz instancję middleware z symulowanym wyjątkiem
            RequestDelegate next = _ => throw new Exception("An unexpected error has occurred.");
            var middleware = new ExceptionHandlingMiddleware(next, _loggerMock.Object);

            // Stwórz żądanie HTTP i ustaw zapis odpowiedzi na strumień pamięci
            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            // Act
            // Przepuść żądanie przez middleware
            await middleware.InvokeAsync(context);

            // Assert
            Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
            Assert.Equal("application/problem+json", context.Response.ContentType);

            // Przesuń kursor na początek w strumieniu odpowiedzi
            context.Response.Body.Seek(0, SeekOrigin.Begin);

            // Sprawdź treść odpowiedzi
            var problemDetails = await JsonSerializer.DeserializeAsync<ProblemDetails>(context.Response.Body);

            Assert.NotNull(problemDetails);
            Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
            Assert.Equal("An unexpected error has occurred.", problemDetails.Detail);
        }
    }
}
