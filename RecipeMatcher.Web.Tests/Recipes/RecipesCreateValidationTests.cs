using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests.Recipes;

public class RecipesCreateValidationTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
// Integration Test >>> Arrange (DB, CLIENT & FORM) -> Act (POST) -> Assert
{
    [Fact]
    public async Task Create_WithEmptyName_DoesNotSave()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var client = Factory.CreateClient();
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

    [Fact]
    public async Task Create_WithNameLongerThan100Characters_DoesNotSave()
    {
        var client = Factory.CreateClient();
        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = new string('a', 101),
            ["PreparationMinutes"] = "10"
        });
        var response = await client.PostAsync("/recipes/create", formData);
        Assert.Contains("field-validation-error", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("0")]
    [InlineData("481")]
    public async Task Create_WithPreparationMinutesOutOfRange_DoesNotSave(string minutes)
    {
        var client = Factory.CreateClient();
        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Soup",
            ["PreparationMinutes"] = minutes
        });
        var response = await client.PostAsync("/recipes/create", formData);
        Assert.Contains("field-validation-error", await response.Content.ReadAsStringAsync());
    }

}