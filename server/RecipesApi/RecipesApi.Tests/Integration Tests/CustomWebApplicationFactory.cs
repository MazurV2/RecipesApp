using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RecipesApi.Tests
{
    public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram> where TProgram : class
    {
        private SqliteConnection _connection;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"));

            builder.ConfigureServices(services =>
            {
                // Usuń istniejący DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>)
                );
                
                if (descriptor != null) services.Remove(descriptor);

                // Utwórz połączenie SQLite w pamięci
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                // Zarejestruj DbContext z SQLite w pamięci
                services.AddDbContext<AppDbContext>(options =>
                {
                    options.UseSqlite(_connection);
                });
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            // Inicjalizuj bazę danych
            using (var scope = host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            return host;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _connection?.Dispose();
        }
    }
}
