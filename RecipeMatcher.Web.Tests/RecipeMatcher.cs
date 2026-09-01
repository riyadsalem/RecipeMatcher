using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
namespace RecipeMatcher.Web.Tests;

public class MatcherTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private async Task<int> SeedIngredientAsync(string name)
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Ingredient? ingredient = await dbContext.Ingredients.FirstOrDefaultAsync(i => i.Name == name);
        if (ingredient == null)
        {
            ingredient = new Ingredient { Name = name };
            dbContext.Ingredients.Add(ingredient);
            await dbContext.SaveChangesAsync();
        }
        return ingredient.Id;
    }

    [Fact]
    public async Task Get_Matcher_ShowsAllIngredients()
    {
        await SeedIngredientAsync("Flour");
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/matcher");
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Flour", content);
    }

    [Fact]
    public async Task Post_Matcher_SortsFullMatchesBeforeNearMatches()
    {
        int eggId = await SeedIngredientAsync("Egg");
        int milkId = await SeedIngredientAsync("Milk");
        int flourId = await SeedIngredientAsync("Flour");
        int butterId = await SeedIngredientAsync("Butter");

        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Recipe pancakes = new()
        {
            Name = "Pancakes",
            PreparationMinutes = 20
        };

        Recipe cake = new()
        {
            Name = "Cake",
            PreparationMinutes = 40
        };
        dbContext.Recipes.AddRange(pancakes, cake);
        await dbContext.SaveChangesAsync();

        dbContext.RecipeIngredients.AddRange(
            new RecipeIngredient
            {
                RecipeId = pancakes.Id,
                IngredientId = eggId
            },
            new RecipeIngredient
            {
                RecipeId = pancakes.Id,
                IngredientId = milkId
            },
            new RecipeIngredient
            {
                RecipeId = pancakes.Id,
                IngredientId = flourId
            },
            new RecipeIngredient
            {
                RecipeId = cake.Id,
                IngredientId = flourId
            },
            new RecipeIngredient
            {
                RecipeId = cake.Id,
                IngredientId = butterId
            });
        await dbContext.SaveChangesAsync();
        var client = Factory.CreateClient();
        var formData = new FormUrlEncodedContent(
            [
                new("ingredientIds", eggId.ToString()),
                new("ingredientIds", milkId.ToString()),
                new("ingredientIds", flourId.ToString())
            ]);
        var response = await client.PostAsync("/matcher", formData);
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Pancakes", content);
        Assert.Contains("Cake", content);
        Assert.Contains("You have everything.", content);
        Assert.Contains("Missing:", content);
    }

    [Fact]
    public async Task Post_Matcher_RecipeWithNoIngredients_ShowsNoIngredientsMessage()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        Recipe recipe = new()
        {
            Name = "Toast",
            PreparationMinutes = 5
        };
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();

        var client = Factory.CreateClient();
        var response = await client.PostAsync("/matcher", new FormUrlEncodedContent([]));
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Toast", content);
        Assert.Contains("No ingredients listed yet.", content);
    }
}