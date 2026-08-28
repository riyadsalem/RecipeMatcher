using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Recipes;

public class RecipesDetailsTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    [Fact]
    public async Task Details_WithExistingId_ReturnsOk()
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        Recipe recipe = new()
        {
            Name = "Waffles",
            PreparationMinutes = 15
        };

        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync($"/recipes/details/{recipe.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Details_WithUnknownId_ReturnsNotFound()
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        var client = factory.CreateClient();
        var response = await client.GetAsync("/recipes/details/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}