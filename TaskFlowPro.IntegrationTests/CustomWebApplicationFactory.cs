using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlowPro.Infrastructure.Data;

namespace TaskFlowPro.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private static string TestConnectionString =>
        Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
        ?? "Host=localhost;Port=5434;Database=taskflowpro_test_db;Username=postgres;Password=postgres";

    // Garantiza que la migración se ejecuta una sola vez aunque múltiples tests
    // intenten levantar la app en paralelo.
    private static readonly object _migrationLock = new();
    private static bool _migrated = false;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.UseSetting("ConnectionStrings:DefaultConnection", TestConnectionString);

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(TestConnectionString));

            lock (_migrationLock)
            {
                if (!_migrated)
                {
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.Migrate();
                    _migrated = true;
                }
            }
        });
    }
}
