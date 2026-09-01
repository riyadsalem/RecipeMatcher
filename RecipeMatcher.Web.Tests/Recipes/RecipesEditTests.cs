using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Recipes;

public class RecipesEditTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<Recipe> CreateRecipeAsync()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Recipe recipe = new()
        {
            Name = "Chicken Curry",
            PreparationMinutes = 12
        };
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();
        return recipe;
    }

    [Fact]
    public async Task Get_Edit_ReturnsCorrectResult()
    {
        Recipe recipe = await CreateRecipeAsync();
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/recipes/edit/{recipe.Id}");
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Chicken Curry", content);

        response = await client.GetAsync("/recipes/edit/1000");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Edit_HandlesValidAndInvalidData()
    {
        Recipe recipe = await CreateRecipeAsync();
        var client = Factory.CreateClient();
        FormUrlEncodedContent formData = new(new Dictionary<string, string>
        {
            ["Id"] = recipe.Id.ToString(),
            ["Name"] = "Chicken and rice",
            ["PreparationMinutes"] = "22"
        });
        await client.PostAsync($"/recipes/edit/{recipe.Id}", formData);

        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Recipe? updated = await dbContext.Recipes.FindAsync(recipe.Id);
        Assert.Equal("Chicken and rice", updated!.Name);
        Assert.Equal(22, updated.PreparationMinutes);

        formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = recipe.Id.ToString(),
            ["Name"] = "",
            ["PreparationMinutes"] = "22"
        });
        await client.PostAsync($"/recipes/edit/{recipe.Id}", formData);

        Recipe? unchanged = await dbContext.Recipes.FindAsync(recipe.Id);
        Assert.Equal("Chicken and rice", unchanged!.Name);
    }

    [Fact]
    public async Task Get_Edit_MarksLinkedIngredient_AsSelected()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Recipe recipe = new()
        {
            Name = "Soup",
            PreparationMinutes = 20
        };

        Ingredient ingredient = new()
        {
            Name = "Carrot"
        };

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
        var response = await client.GetAsync($"/recipes/edit/{recipe.Id}");
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Carrot", content);
        Assert.Contains("checked", content);
    }
}