
namespace RecipesApi.Tests.Integration_Tests
{
    public class ExceptionHandlingMiddlewareTests : BaseIntegrationTests, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        public ExceptionHandlingMiddlewareTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

        // Test middleware do obsługi wyjątków
        [Fact]
        public async Task GetTestError_ShouldReturnInternalServerError()
        {
            // Arrange
            var url = "/api/Recipe/test-error";

            // Act
            var response = await _unauthorizedClient.GetAsync(url);
            
            // Assert
            Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("To jest błąd testowy.", content);
        }
    }
}
