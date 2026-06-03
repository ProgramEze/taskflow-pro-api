using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using TaskFlowPro.IntegrationTests.Helpers;

namespace TaskFlowPro.IntegrationTests.Api;

public class TaskAssignIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TaskAssignIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers privados
    // ─────────────────────────────────────────────────────────────────────────

    // Registra un usuario y devuelve (client autenticado, userId, email).
    private async Task<(HttpClient client, Guid userId, string email)> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();

        var email = $"user_{Guid.NewGuid():N}@example.com";
        var registerRequest = new
        {
            firstName = "Test",
            lastName = "User",
            email,
            password = "Test1234!"
        };

        var registerResponse = await client.PostAsJsonAsync("/api/Auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();

        var registerJson = JsonDocument.Parse(await registerResponse.Content.ReadAsStringAsync());
        var token = registerJson.RootElement.GetProperty("token").GetString()!;
        var userId = registerJson.RootElement.GetProperty("userId").GetGuid();

        IntegrationTestHelper.SetBearerToken(client, token);

        return (client, userId, email);
    }

    // Registra un owner, crea workspace + proyecto y devuelve todo lo necesario.
    private async Task<(HttpClient ownerClient, Guid ownerId, Guid workspaceId, Guid projectId)>
        CreateOwnerWithProjectAsync()
    {
        var (ownerClient, ownerId, _) = await CreateAuthenticatedClientAsync();

        var workspaceResponse = await ownerClient.PostAsJsonAsync("/api/Workspaces", new
        {
            name = "Workspace de asignación",
            description = "Workspace para tests de asignación"
        });
        workspaceResponse.EnsureSuccessStatusCode();
        var workspaceJson = JsonDocument.Parse(await workspaceResponse.Content.ReadAsStringAsync());
        var workspaceId = workspaceJson.RootElement.GetProperty("id").GetGuid();

        var projectResponse = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/projects", new
            {
                name = "Proyecto de asignación",
                description = "Proyecto para tests de asignación"
            });
        projectResponse.EnsureSuccessStatusCode();
        var projectJson = JsonDocument.Parse(await projectResponse.Content.ReadAsStringAsync());
        var projectId = projectJson.RootElement.GetProperty("id").GetGuid();

        return (ownerClient, ownerId, workspaceId, projectId);
    }

    // Crea una tarea y devuelve su Id.
    private async Task<Guid> CreateTaskAndGetIdAsync(HttpClient client, Guid projectId,
        string title = "Tarea de asignación")
    {
        var response = await client.PostAsJsonAsync(
            $"/api/projects/{projectId}/tasks", new
            {
                title,
                description = "Descripción",
                priority = 1
            });
        response.EnsureSuccessStatusCode();
        var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("id").GetGuid();
    }

    // Registra un segundo usuario y lo agrega al workspace como miembro por email.
    // Devuelve (client del miembro, userId del miembro).
    private async Task<(HttpClient memberClient, Guid memberId)>
        AddMemberToWorkspaceAsync(HttpClient ownerClient, Guid workspaceId)
    {
        var (memberClient, memberId, memberEmail) = await CreateAuthenticatedClientAsync();

        var addResponse = await ownerClient.PostAsJsonAsync(
            $"/api/workspaces/{workspaceId}/members", new
            {
                email = memberEmail
            });
        addResponse.EnsureSuccessStatusCode();

        return (memberClient, memberId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PATCH /api/tasks/{taskId}/assign — casos felices
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Assign_OwnerAssignsAnotherMember_ReturnsOkWithAssignedUserId()
    {
        var (ownerClient, _, workspaceId, projectId) = await CreateOwnerWithProjectAsync();
        var (_, memberId) = await AddMemberToWorkspaceAsync(ownerClient, workspaceId);
        var taskId = await CreateTaskAndGetIdAsync(ownerClient, projectId);

        var response = await ownerClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/assign",
            new { assignedUserId = memberId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("assignedUserId").GetGuid().Should().Be(memberId);
    }

    [Fact]
    public async Task Assign_MemberSelfAssigns_ReturnsOkWithAssignedUserId()
    {
        var (ownerClient, _, workspaceId, projectId) = await CreateOwnerWithProjectAsync();
        var (memberClient, memberId) = await AddMemberToWorkspaceAsync(ownerClient, workspaceId);
        var taskId = await CreateTaskAndGetIdAsync(ownerClient, projectId);

        var response = await memberClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/assign",
            new { assignedUserId = memberId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("assignedUserId").GetGuid().Should().Be(memberId);
    }

    [Fact]
    public async Task Assign_OwnerSelfAssigns_ReturnsOk()
    {
        var (ownerClient, ownerId, _, projectId) = await CreateOwnerWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(ownerClient, projectId);

        var response = await ownerClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/assign",
            new { assignedUserId = ownerId });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);
        json.RootElement.GetProperty("assignedUserId").GetGuid().Should().Be(ownerId);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PATCH /api/tasks/{taskId}/assign — errores de autenticación / autorización
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Assign_WithoutToken_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/assign",
            new { assignedUserId = Guid.NewGuid() });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Assign_MemberAssignsAnotherUser_ReturnsForbidden()
    {
        var (ownerClient, _, workspaceId, projectId) = await CreateOwnerWithProjectAsync();
        var (memberClient, _) = await AddMemberToWorkspaceAsync(ownerClient, workspaceId);
        var (_, otherMemberId) = await AddMemberToWorkspaceAsync(ownerClient, workspaceId);
        var taskId = await CreateTaskAndGetIdAsync(ownerClient, projectId);

        // Un miembro intenta asignar a otro usuario (no a sí mismo) → 403
        var response = await memberClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/assign",
            new { assignedUserId = otherMemberId });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Assign_UserNotInWorkspace_ReturnsForbidden()
    {
        var (ownerClient, _, _, projectId) = await CreateOwnerWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(ownerClient, projectId);

        // Usuario autenticado pero que no pertenece al workspace
        var (outsiderClient, outsiderId, _) = await CreateAuthenticatedClientAsync();

        var response = await outsiderClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/assign",
            new { assignedUserId = outsiderId });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PATCH /api/tasks/{taskId}/assign — errores de negocio
    // ─────────────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Assign_TaskNotFound_ReturnsNotFound()
    {
        var (ownerClient, ownerId, _, _) = await CreateOwnerWithProjectAsync();

        var response = await ownerClient.PatchAsJsonAsync(
            $"/api/tasks/{Guid.NewGuid()}/assign",
            new { assignedUserId = ownerId });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Assign_TaskAlreadyAssigned_ReturnsConflict()
    {
        var (ownerClient, ownerId, _, projectId) = await CreateOwnerWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(ownerClient, projectId);

        // Primera asignación
        var first = await ownerClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/assign",
            new { assignedUserId = ownerId });
        first.EnsureSuccessStatusCode();

        // Segunda asignación sobre la misma tarea → 409
        var second = await ownerClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/assign",
            new { assignedUserId = ownerId });

        second.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Assign_UserNotMemberOfWorkspace_ReturnsBadRequest()
    {
        var (ownerClient, _, _, projectId) = await CreateOwnerWithProjectAsync();
        var taskId = await CreateTaskAndGetIdAsync(ownerClient, projectId);

        // Owner intenta asignar a un usuario que no está en el workspace
        var (_, outsiderId, _) = await CreateAuthenticatedClientAsync();

        var response = await ownerClient.PatchAsJsonAsync(
            $"/api/tasks/{taskId}/assign",
            new { assignedUserId = outsiderId });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
