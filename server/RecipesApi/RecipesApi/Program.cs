using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using RecipesApi;
using RecipesApi.Filters;
using RecipesApi.Services;
using RecipesApi.Services.Interfaces;
using RecipesApi.Settings;
using RecipesApi.Validators.Auth;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers(options => 
{
    options.Filters.Add<ValidationFilter>();
});

// Zarejestruj serwisy
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Przekaż ustawienia JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// Walidatory DTO
builder.Services.AddValidatorsFromAssemblyContaining<RegisterDTOValidator>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    // Zmodyfikuj domyślnie wygenerowany plik
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Zdefiniuj autoryzację (JWT w nagłówku)
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            In = ParameterLocation.Header,
            BearerFormat = "JWT",
            Name = "Authorization",
            Description = "Wprowadź token JWT"
        };

        // Zapisz definicję pod kluczem "Bearer"
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = scheme;

        // Dodaj definicję jako wymóg dla bezpieczeństwa
        var requirement = new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
        };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(requirement);

        return Task.CompletedTask;
    });
});

// AppDbContext configuration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Weryfikacja tokena
builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        // Ustal reguły walidacji tokenu
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey
            (
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:Secret"])
            )
        };
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    // Scalar
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
