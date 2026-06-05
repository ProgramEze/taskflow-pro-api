using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskFlowPro.Infrastructure.Data;

namespace TaskFlowPro.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Es la cadena de conexión a la base de datos de pruebas. Hay que asegurarse de que esta base de datos exista previamente y esté configurada correctamente.
    private const string TestConnectionString =
        "Host=localhost;Port=5434;Database=taskflowpro_test_db;Username=postgres;Password=postgres";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Configura el entorno de la aplicación para que use el entorno de desarrollo. Esto es importante para asegurarse de que se apliquen las configuraciones específicas del entorno, como la cadena de conexión a la base de datos de pruebas.
        builder.UseEnvironment("Development");

        // Configura los servicios de la aplicación para que utilicen la base de datos de pruebas en lugar de la base de datos de producción. Esto se hace eliminando cualquier configuración existente de DbContext y agregando una nueva configuración que apunte a la base de datos de pruebas.
        builder.ConfigureServices(services =>
        {
            // Busca y elimina cualquier descriptor de servicio que corresponda a DbContextOptions<AppDbContext>. Esto es necesario para asegurarse de que la configuración de la base de datos de producción no interfiera con la configuración de la base de datos de pruebas.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));

            // Si se encuentra un descriptor, se elimina de la colección de servicios.
            if (descriptor != null)
                services.Remove(descriptor);

            // Agrega una nueva configuración de DbContext que apunte a la base de datos de pruebas utilizando la cadena de conexión definida anteriormente. Esto asegura que todas las operaciones de la base de datos durante las pruebas se realicen en la base de datos de pruebas en lugar de la base de datos de producción.
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(TestConnectionString));

            // Construye el proveedor de servicios y crea un alcance para obtener una instancia de AppDbContext. Luego, se llama al método Migrate() para aplicar cualquier migración pendiente a la base de datos de pruebas. Esto asegura que la base de datos de pruebas esté actualizada con el esquema más reciente antes de ejecutar las pruebas.
            var sp = services.BuildServiceProvider();

            // Crea un alcance para obtener una instancia de AppDbContext y aplicar las migraciones pendientes a la base de datos de pruebas.
            using var scope = sp.CreateScope();

            // Obtiene una instancia de AppDbContext desde el proveedor de servicios y llama al método Migrate() para aplicar cualquier migración pendiente a la base de datos de pruebas.
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Aplica cualquier migración pendiente a la base de datos de pruebas para asegurarse de que esté actualizada con el esquema más reciente antes de ejecutar las pruebas.
            db.Database.Migrate();
        });
    }
}