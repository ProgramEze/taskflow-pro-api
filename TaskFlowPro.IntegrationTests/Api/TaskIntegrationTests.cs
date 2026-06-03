using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlowPro.IntegrationTests.Helpers;

namespace TaskFlowPro.IntegrationTests.Api;

public class TaskIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TaskIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper privado: registra un usuario, crea workspace + proyecto y
    // devuelve (client con token, workspaceId, projectId).
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<(HttpClient client, Guid workspaceId, Guid projectId)>
        CreateAuthenticatedClientWithProjectAsync()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var workspaceResponse = await client.PostAsJsonAsync("/api/Workspaces", new
        {
            name = "Workspace para tareas",
            description = "Workspace de soporte para tests de tareas"
        });
        workspaceResponse.EnsureSuccessStatusCode();
        var workspaceJson = JsonDocument.Parse(await workspaceResponse.Content.ReadAsStringAsync());
        var workspaceId = workspaceJson.RootElement.GetProperty("id").GetGuid();

        var projectResponse = await client.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects", new
            {
                name = "Proyecto para tareas",
                description = "Proyecto de soporte para tests de tareas"
            });
        projectResponse.EnsureSuccessStatusCode();
        var projectJson = JsonDocument.Parse(await projectResponse.Content.ReadAsStringAsync());
        var projectId = projectJson.RootElement.GetProperty("id").GetGuid();

        return (client, workspaceId, projectId);
    }

    // Helper privado: crea una tarea dentro del proyecto y devuelve su Id.
    private async Task<Guid> CreateTaskAndGetIdAsync(HttpClient client, Guid projectId,
        string title = "Tarea de prueba")
    {
        var response = await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/tasks", new
            {
                title,
                description = "Descripción de la tarea",
                priority = 1  // Medium
            });
        response.EnsureSuccessStatusCode();
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("id").GetGuid();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // POST /api/projects/{projectId}/tasks
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateTask_WithValidData_ReturnsCreated()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();

        var request = new
        {
            title = "Mi Tarea",
            description = "Descripción de prueba",
            priority = 1  // Medium
        };

        var response = await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Mi Tarea");
    }

    [Fact]
    public async Task CreateTask_WithDueDate_ReturnsCreatedWithDueDate()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();

        var request = new
        {
            title = "Tarea con fecha",
            priority = 0,  // Low
            dueDate = DateTime.UtcNow.AddDays(7).ToString("o")  // ISO 8601
        };

        var response = await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Tarea con fecha");
    }

    [Fact]
    public async Task CreateTask_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var request = new { title = "Tarea sin token", priority = 1 };

        var response = await client.PostAsJsonAsync(
            $"/api/projects/{Guid.NewGuid()}/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateTask_WithEmptyTitle_ReturnsBadRequest()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();

        var request = new { title = "", priority = 1 };

        var response = await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateTask_InNonExistentProject_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var request = new { title = "Tarea en proyecto inexistente", priority = 1 };

        var response = await client.PostAsJsonAsync(
            $"/api/projects/{Guid.NewGuid()}/tasks", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/projects/{projectId}/tasks
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTasksByProject_WithToken_ReturnsOk()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();

        await CreateTaskAndGetIdAsync(client, projectId, "Tarea para listar");

        var response = await client.GetAsync($"/api/projects/{projectId}/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Tarea para listar");
    }

    [Fact]
    public async Task GetTasksByProject_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTasksByProject_InNonExistentProject_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}/tasks");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET /api/tasks/{taskId}
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetTaskById_WithValidId_ReturnsOk()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(client, projectId, "Tarea por ID");

        var response = await client.GetAsync($"/api/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Tarea por ID");
    }

    [Fact]
    public async Task GetTaskById_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetTaskById_WithNonExistentId_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var response = await client.GetAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PATCH /api/tasks/{taskId}/status
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangeStatus_WithValidStatus_ReturnsOk()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(client, projectId);

        var request = new { status = 1 };  // InProgress

        var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/status", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("1");  // status: InProgress
    }

    [Fact]
    public async Task ChangeStatus_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var request = new { status = 1 };

        var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/status", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ChangeStatus_OnNonExistentTask_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var request = new { status = 1 };

        var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/status", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PATCH /api/tasks/{taskId}/priority
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ChangePriority_WithValidPriority_ReturnsOk()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(client, projectId);

        var request = new { priority = 2 };  // High

        var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/priority", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("2");  // priority: High
    }

    [Fact]
    public async Task ChangePriority_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var request = new { priority = 2 };

        var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/priority", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PUT /api/tasks/{taskId}
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateTask_WithValidData_ReturnsOk()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(client, projectId);

        var request = new
        {
            title = "Título actualizado",
            description = "Descripción actualizada"
        };

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{taskId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Título actualizado");
    }

    [Fact]
    public async Task UpdateTask_WithEmptyTitle_ReturnsBadRequest()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(client, projectId);

        var request = new { title = "", description = "Sin título" };

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{taskId}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateTask_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var request = new { title = "Sin token", description = "" };

        var response = await client.PutAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE /api/tasks/{taskId}
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteTask_WithValidId_ReturnsNoContent()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(client, projectId);

        var response = await client.DeleteAsync($"/api/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteTask_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeleteTask_WithNonExistentId_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var token = await IntegrationTestHelper.RegisterAndGetTokenAsync(client);
        IntegrationTestHelper.SetBearerToken(client, token);

        var response = await client.DeleteAsync($"/api/tasks/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteTask_AlreadyDeleted_ReturnsNotFound()
    {
        var (client, _, projectId) = await CreateAuthenticatedClientWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(client, projectId);

        await client.DeleteAsync($"/api/tasks/{taskId}");

        // Segundo intento: la tarea ya no está activa
        var response = await client.DeleteAsync($"/api/tasks/{taskId}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
