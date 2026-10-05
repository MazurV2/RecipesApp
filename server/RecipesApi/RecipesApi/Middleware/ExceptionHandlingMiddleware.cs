using Microsoft.AspNetCore.Mvc;

namespace RecipesApi.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Wystąpił nieobsłużony wyjątek: {Message}.", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = ex switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                _ => StatusCodes.Status500InternalServerError
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = statusCode;
            
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = statusCode switch
                {
                    StatusCodes.Status401Unauthorized => "Unauthorized",
                    StatusCodes.Status404NotFound => "Not Found",
                    _ => "Internal Server Error"
                },
                Detail = ex.Message
            };

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
