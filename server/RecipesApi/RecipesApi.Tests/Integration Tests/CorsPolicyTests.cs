
using System.Net;

namespace RecipesApi.Tests.Integration_Tests
{
    public class CorsPolicyTests : BaseIntegrationTests, IClassFixture<CustomWebApplicationFactory<Program>>
    {
        public CorsPolicyTests(CustomWebApplicationFactory<Program> factory) : base(factory) { }

        // Test dozwolonego pochodzenia CORS
        [Fact]
        public async Task GetRecipes_AllowedCors_ShouldReturnWithCorsHeaders()
        {
            // Arrange
            var url = "/api/Recipe";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Origin", "http://localhost:3000");
            
            // Act
            var response = await _unauthorizedClient.SendAsync(request);
            
            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Sprawdź, czy nagłówki CORS są obecne i poprawne
            Assert.True(response.Headers.Contains("Access-Control-Allow-Origin"));
            Assert.Equal("http://localhost:3000", response.Headers.GetValues("Access-Control-Allow-Origin").FirstOrDefault());
        }

        [Fact]
        public async Task GetRecipes_DisallowedCors_ShouldReturnWithoutCorsHeaders()
        {
            // Arrange
            var url = "/api/Recipe";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Origin", "http://niedozwolona-domena.com");

            // Act
            var response = await _unauthorizedClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Sprawdź, czy nagłówki CORS nie są obecne
            Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
        }

    }
}
