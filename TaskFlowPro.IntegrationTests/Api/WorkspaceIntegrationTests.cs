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

    [Fact]
    public async Task GetMyWorkspaces_WithToken_ReturnsOk()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        await client.PostAsJsonAsync("/api/Workspaces", new
        {
            name = "Workspace listado",
            description = "Para verificar el listado"
        });

        var response = await client.GetAsync("/api/Workspaces");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
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