using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlowPro.IntegrationTests.Helpers;

namespace TaskFlowPro.IntegrationTests.Api;

public class ProjectIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ProjectIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ─────────────────────────────────────────────────────────────
    // Helper privado: registra un usuario, crea un workspace y
    // devuelve (client con token, workspaceId).
    // ─────────────────────────────────────────────────────────────
    private async Task<(HttpClient client, Guid workspaceId)> CreateAuthenticatedClientWithWorkspaceAsync()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var workspaceRequest = new
        {
            name = "Workspace para proyectos",
            description = "Workspace de soporte para tests de proyectos"
        };

        var workspaceResponse = await client.PostAsJsonAsync("/api/Workspaces", workspaceRequest);
        workspaceResponse.EnsureSuccessStatusCode();

        var body = await workspaceResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        var workspaceId = json.RootElement.GetProperty("id").GetGuid();

        return (client, workspaceId);
    }

    // ─────────────────────────────────────────────────────────────
    // POST /api/workspaces/{workspaceId}/projects
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateProject_WithValidData_ReturnsCreated()
    {
        var (client, workspaceId) = await CreateAuthenticatedClientWithWorkspaceAsync();

        var request = new
        {
            name = "Mi Proyecto",
            description = "Proyecto de prueba"
        };

        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Mi Proyecto");
    }

    [Fact]
    public async Task CreateProject_WithDueDate_ReturnsCreatedWithDueDate()
    {
        var (client, workspaceId) = await CreateAuthenticatedClientWithWorkspaceAsync();

        var dueDate = DateTime.UtcNow.AddDays(30);

        var request = new
        {
            name = "Proyecto con fecha",
            description = "Tiene fecha de vencimiento",
            dueDate
        };

        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Proyecto con fecha");
    }

    [Fact]
    public async Task CreateProject_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var workspaceId = Guid.NewGuid();

        var request = new
        {
            name = "Proyecto sin token",
            description = "No deberia crearse"
        };

        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProject_WithEmptyName_ReturnsBadRequest()
    {
        var (client, workspaceId) = await CreateAuthenticatedClientWithWorkspaceAsync();

        var request = new
        {
            name = "",
            description = "Proyecto sin nombre"
        };

        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProject_InNonExistentWorkspace_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var request = new
        {
            name = "Proyecto en workspace inexistente",
            description = "El workspace no existe"
        };

        var response = await client.PostAsJsonAsync(
            $"/api/workspaces/{Guid.NewGuid()}/projects", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/workspaces/{workspaceId}/projects
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProjectsByWorkspace_WithToken_ReturnsOk()
    {
        var (client, workspaceId) = await CreateAuthenticatedClientWithWorkspaceAsync();

        await client.PostAsJsonAsync($"/api/workspaces/{workspaceId}/projects", new
        {
            name = "Proyecto listado",
            description = "Para verificar el listado"
        });

        var response = await client.GetAsync($"/api/workspaces/{workspaceId}/projects");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Proyecto listado");
    }

    [Fact]
    public async Task GetProjectsByWorkspace_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        var workspaceId = Guid.NewGuid();

        var response = await client.GetAsync($"/api/workspaces/{workspaceId}/projects");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProjectsByWorkspace_InNonExistentWorkspace_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var response = await client.GetAsync($"/api/workspaces/{Guid.NewGuid()}/projects");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────
    // GET /api/projects/{projectId}
    // ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProjectById_WithValidId_ReturnsOk()
    {
        var (client, workspaceId) = await CreateAuthenticatedClientWithWorkspaceAsync();

        var createResponse = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects",
            new { name = "Proyecto por ID", description = "Para buscar por ID" });

        createResponse.EnsureSuccessStatusCode();

        var createBody = await createResponse.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(createBody);
        var projectId = json.RootElement.GetProperty("id").GetGuid();

        var response = await client.GetAsync($"/api/projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Proyecto por ID");
    }

    [Fact]
    public async Task GetProjectById_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProjectById_WithNonExistentId_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
