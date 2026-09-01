using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Recipes;

public class RecipesDetailsTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Details_WithExistingId_ReturnsOk()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Recipe recipe = new()
        {
            Name = "Waffles",
            PreparationMinutes = 15
        };

        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();
        var client = Factory.CreateClient();
        var response = await client.GetAsync($"/recipes/details/{recipe.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Details_WithUnknownId_ReturnsNotFound()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/recipes/details/1");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Details_ShowsRelatedIngredients()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Recipe recipe = new() { Name = "Salad", PreparationMinutes = 5 };
        Ingredient ingredient = new() { Name = "Tomatoes" };

        dbContext.Recipes.Add(recipe);
        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync();

        dbContext.RecipeIngredients.Add(new RecipeIngredient
        {
            RecipeId = recipe.Id,
            IngredientId = ingredient.Id
        });
        await dbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var response = await client.GetAsync($"/recipes/details/{recipe.Id}");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Tomatoes", content);
    }

}