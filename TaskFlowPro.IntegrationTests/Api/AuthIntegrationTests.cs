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

    [Fact]
    public async Task Register_WithValidData_ReturnsCreated()
    {
        var client = _factory.CreateClient();

        var request = new
        {
            firstName = "Test",
            lastName = "User",
            email = $"test_{Guid.NewGuid():N}@example.com",
            password = "Test1234!"
        };

        var response = await client.PostAsJsonAsync("/api/Auth/register", request);

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