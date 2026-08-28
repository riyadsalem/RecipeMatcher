using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests;

public class RecipesCreateValidationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
// Integration Test >>> Arrange (DB, CLIENT & FORM) -> Act (POST) -> Assert
{
    [Fact]
    public async Task Create_WithEmptyName_DoesNotSave()
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var client = factory.CreateClient();
        FormUrlEncodedContent formData = new(new Dictionary<string, string>
        {
            ["Name"] = "",
            ["PreparationMinutes"] = "10"
        });

        var response = await client.PostAsync("/recipes/create", formData);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("field-validation-error", content);

        int count = await dbContext.Recipes.CountAsync();
        Assert.Equal(0, count);
    }
}