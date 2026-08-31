using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
namespace RecipeMatcher.Web.Tests;

public class MatcherTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<int> SeedIngredientAsync(string name)
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
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
        var client = factory.CreateClient();
        var response = await client.GetAsync("/matcher");
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Flour", content);
    }

    [Fact]
    public async Task Post_Matcher_ReturnsOnlyFullMatches()
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();

        int eggId = await SeedIngredientAsync("Egg");
        int milkId = await SeedIngredientAsync("Milk");
        int flourId = await SeedIngredientAsync("Flour");
        int butterId = await SeedIngredientAsync("Butter");

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

        var client = factory.CreateClient();
        var formData = new FormUrlEncodedContent(
            [
                new("ingredientIds", eggId.ToString()),
                new("ingredientIds", milkId.ToString()),
                new("ingredientIds", flourId.ToString())
            ]);
        var response = await client.PostAsync("/matcher", formData);
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Pancakes", content); //.Where(recipe => recipe.RecipeIngredients.All(r => ingredientIds.Contains(r.IngredientId))) => IN MatcherController
        Assert.DoesNotContain("Cake", content);
    }

    [Fact]
    public async Task Post_Matcher_RecipeWithNoIngredients_IsAlwaysAMatch()
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Recipe recipe = new()
        {
            Name = "Toast",
            PreparationMinutes = 5
        };
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();

        var client = factory.CreateClient();
        var response = await client.PostAsync("/matcher", new FormUrlEncodedContent([]));
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Toast", content);
    }
}