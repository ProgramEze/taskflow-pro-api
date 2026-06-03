using System.Net.Http.Json;
using System.Text.Json;

namespace TaskFlowPro.IntegrationTests.Helpers;

public static class IntegrationTestHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static async Task<string> RegisterAndGetTokenAsync(HttpClient client)
    {
        var request = new
        {
            firstName = "Test",
            lastName = "User",
            email = $"user_{Guid.NewGuid():N}@example.com",
            password = "Test1234!"
        };

        var response = await client.PostAsJsonAsync("/api/Auth/register", request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(body);

        return json.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Token no encontrado en la respuesta.");
    }

    public static void SetBearerToken(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}