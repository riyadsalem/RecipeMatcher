using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Recipes;

public class RecipesDeleteTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<Recipe> CreateRecipeAsync()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Recipe recipe = new Recipe
        {
            Name = "Pasta",
            PreparationMinutes = 25
        };
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();
        return recipe;
    }

    [Fact]
    public async Task Get_Delete_ReturnsCorrectResult()
    {
        Recipe recipe = await CreateRecipeAsync();
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/recipes/delete/{recipe.Id}");
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Pasta", content);

        response = await client.GetAsync("/recipes/delete/1000");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Delete_ReturnsCorrectResult()
    {
        Recipe recipe = await CreateRecipeAsync();
        var client = Factory.CreateClient();

        FormUrlEncodedContent formData = new(new Dictionary<string, string> { ["Id"] = recipe.Id.ToString() });
        await client.PostAsync($"/recipes/delete/{recipe.Id}", formData);

        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Recipe? deleted = await dbContext.Recipes.FindAsync(recipe.Id);
        Assert.Null(deleted);

        formData = new FormUrlEncodedContent(new Dictionary<string, string> { ["Id"] = "1000" });
        var response = await client.PostAsync("/recipes/delete/1000", formData);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}