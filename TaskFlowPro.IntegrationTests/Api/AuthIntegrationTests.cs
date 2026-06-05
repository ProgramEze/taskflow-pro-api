using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace TaskFlowPro.IntegrationTests.Api;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // Prueba de integración para el endpoint de registro de usuarios. Verifica que al enviar datos válidos, la API responda con un código de estado HTTP 201 Created, lo que indica que el usuario se registró correctamente.
    [Fact]
    // Esta prueba de integración verifica que el endpoint de registro de usuarios funcione correctamente cuando se envían datos válidos. Crea un cliente HTTP utilizando la fábrica personalizada, define un objeto de solicitud con datos válidos para registrar un nuevo usuario, envía una solicitud POST a la ruta "/api/Auth/register" y verifica que la respuesta tenga un código de estado HTTP 201 Created.
    public async Task Register_WithValidData_ReturnsCreated()
    {
        // Crea un cliente HTTP utilizando la fábrica personalizada para enviar solicitudes a la API durante las pruebas de integración.
        var client = _factory.CreateClient();

        // Define un objeto de solicitud con datos válidos para registrar un nuevo usuario. Esto incluye el nombre, apellido, correo electrónico y contraseña del usuario. El correo electrónico se genera de forma única utilizando un GUID para evitar conflictos con otros usuarios registrados durante las pruebas.
        var request = new
        {
            firstName = "Test",
            lastName = "User",
            email = $"test_{Guid.NewGuid():N}@example.com",
            password = "Test1234!"
        };

        // Envía una solicitud POST a la ruta "/api/Auth/register" con el objeto de solicitud como contenido JSON. Esto simula el proceso de registro de un nuevo usuario en la API.
        var response = await client.PostAsJsonAsync("/api/Auth/register", request);

        // Verifica que la respuesta tenga un código de estado HTTP 201 Created, lo que indica que el usuario se registró correctamente en la API.
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ReturnsConflict()
    {
        var client = _factory.CreateClient();
        var email = $"duplicate_{Guid.NewGuid():N}@example.com";

        await client.PostAsJsonAsync("/api/Auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = "Test1234!"
        });

        var response = await client.PostAsJsonAsync("/api/Auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = "Test1234!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithEmptyPassword_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            firstName = "Test",
            lastName = "User",
            email = $"test_{Guid.NewGuid():N}@example.com",
            password = ""
        };

        var response = await client.PostAsJsonAsync("/api/Auth/register", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenInBody()
    {
        var client = _factory.CreateClient();
        var email = $"login_{Guid.NewGuid():N}@example.com";
        var password = "Test1234!";

        await client.PostAsJsonAsync("/api/Auth/register", new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password
        });

        var response = await client.PostAsJsonAsync("/api/Auth/login", new
        {
            email,
            password
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("token");
    }
}