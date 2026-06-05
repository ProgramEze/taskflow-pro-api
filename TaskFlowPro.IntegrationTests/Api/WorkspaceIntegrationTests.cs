using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TaskFlowPro.IntegrationTests.Helpers;

namespace TaskFlowPro.IntegrationTests.Api;

public class WorkspaceIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public WorkspaceIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateWorkspace_WithValidData_ReturnsCreated()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var request = new
        {
            name = "Mi Workspace",
            description = "Workspace de prueba"
        };

        var response = await client.PostAsJsonAsync("/api/Workspaces", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Mi Workspace");
    }

    [Fact]
    public async Task CreateWorkspace_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            name = "Workspace sin token",
            description = "No deberia crearse"
        };

        var response = await client.PostAsJsonAsync("/api/Workspaces", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateWorkspace_WithEmptyName_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var request = new
        {
            name = "",
            description = "Workspace sin nombre"
        };

        var response = await client.PostAsJsonAsync("/api/Workspaces", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // Prueba de integración para verificar que el endpoint de creación de workspaces funcione correctamente cuando se envían datos válidos. Crea un cliente HTTP utilizando la fábrica personalizada, obtiene un token de autenticación, establece el token en el cliente, define un objeto de solicitud con datos válidos para crear un nuevo workspace, envía una solicitud POST a la ruta "/api/Workspaces" y verifica que la respuesta tenga un código de estado HTTP 201 Created y que el cuerpo de la respuesta contenga el nombre del workspace creado.
    [Fact]
    // Esta prueba de integración verifica que el endpoint de creación de workspaces funcione correctamente cuando se envían datos válidos. Crea un cliente HTTP utilizando la fábrica personalizada, obtiene un token de autenticación, establece el token en el cliente, define un objeto de solicitud con datos válidos para crear un nuevo workspace, envía una solicitud POST a la ruta "/api/Workspaces" y verifica que la respuesta tenga un código de estado HTTP 201 Created y que el cuerpo de la respuesta contenga el nombre del workspace creado.
    public async Task GetMyWorkspaces_WithToken_ReturnsOk()
    {
        // Crea un cliente HTTP utilizando la fábrica personalizada para enviar solicitudes a la API durante las pruebas de integración.
        var client = _factory.CreateClient();

        // Obtiene un token de autenticación para un usuario registrado utilizando un helper de integración. Esto es necesario para acceder a los endpoints protegidos de la API durante las pruebas de integración.
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);

        // Establece el token de autenticación en el cliente HTTP para que se incluya en las solicitudes enviadas a la API durante las pruebas de integración. Esto permite acceder a los endpoints protegidos de la API.
        IntegrationTestHelper.SetBearerToken(client, token);

        // Define un objeto de solicitud con datos válidos para crear un nuevo workspace. Esto incluye el nombre y la descripción del workspace que se desea crear.
        await client.PostAsJsonAsync("/api/Workspaces", new
        {
            name = "Workspace listado",
            description = "Para verificar el listado"
        });

        // Envía una solicitud GET a la ruta "/api/Workspaces" para obtener la lista de workspaces del usuario autenticado. Esto simula el proceso de obtener los workspaces asociados al usuario en la API.
        var response = await client.GetAsync("/api/Workspaces");

        // Verifica que la respuesta tenga un código de estado HTTP 200 OK, lo que indica que la solicitud se procesó correctamente y se obtuvo la lista de workspaces del usuario autenticado.
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Lee el cuerpo de la respuesta como una cadena y verifica que contenga el nombre del workspace creado anteriormente. Esto asegura que el workspace creado se encuentra en la lista de workspaces del usuario autenticado.
        var body = await response.Content.ReadAsStringAsync();

        // Verifica que el cuerpo de la respuesta contenga el nombre del workspace creado anteriormente, lo que indica que el workspace se encuentra en la lista de workspaces del usuario autenticado.
        body.Should().Contain("Workspace listado");
    }

    [Fact]
    public async Task GetMyWorkspaces_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/Workspaces");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}