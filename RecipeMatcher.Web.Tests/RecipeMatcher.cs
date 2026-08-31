using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests;

public class MatcherTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
    private async Task<int> SeedIngredientAsync(string name)
    {
        using var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Ingredient ingredient = new() { Name = name };
        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync();
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
    public async Task Post_Matcher_WithSelectedIds_ReturnsThemInResponse()
    {
        int id = await SeedIngredientAsync("Sugar");

        var client = factory.CreateClient();
        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["ingredientIds"] = id.ToString()
        });
        var response = await client.PostAsync("/matcher", formData);
        string content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"<li>{id}</li>", content);
    }

    [Fact]
    public async Task Post_Matcher_WithNoSelection_DoesNotFail()
    {
        await SeedIngredientAsync("Butter");

        var client = factory.CreateClient();
        var response = await client.PostAsync("/matcher", new FormUrlEncodedContent([]));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
