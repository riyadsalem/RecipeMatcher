using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Ingredients;

public class IngredientsCreateValidationTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
// Zoals (RecipesTests files)
{
    [Fact]
    public async Task Create_WithEmptyName_DoesNotSave()
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        int countBefore = await dbContext.Ingredients.CountAsync();

        var client = factory.CreateClient();
        FormUrlEncodedContent formData = new(new Dictionary<string, string>
        {
            ["Name"] = ""
        });

        var response = await client.PostAsync("/ingredients/create", formData);
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("field-validation-error", content);

        int countAfter = await dbContext.Ingredients.CountAsync();
        Assert.Equal(countBefore, countAfter);
    }

    [Fact]
    public async Task Create_WithDuplicateName_DoesNotSave()
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        dbContext.Ingredients.Add(new Ingredient { Name = "Salt" });
        await dbContext.SaveChangesAsync();

        var client = factory.CreateClient();
        FormUrlEncodedContent formData = new(new Dictionary<string, string>
        {
            ["Name"] = "Salt"
        });

        var response = await client.PostAsync("/ingredients/create", formData);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        int count = await dbContext.Ingredients.CountAsync(ingredient => ingredient.Name == "Salt");
        Assert.Equal(1, count);
    }
}