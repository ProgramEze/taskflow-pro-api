using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;

namespace TaskFlowPro.IntegrationTests.Api;

public class SwaggerTests
{
    [Fact]
    public async Task SwaggerJson_ShouldReturnOk_WhenApplicationStartsInDevelopment()
    {
        // Arrange
        await using var application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
            });

        var client = application.CreateClient();

        // Act
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}