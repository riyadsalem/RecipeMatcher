using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Ingredients;

public class IngredientsEditTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
// Zoals (RecipesTests files)
{
    private async Task<Ingredient> CreateIngredientAsync()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Ingredient ingredient = new()
        {
            Name = "Pepper"
        };
        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync();
        return ingredient;
    }

    [Fact]
    public async Task Get_Edit_ReturnsCorrectResult()
    {
        Ingredient ingredient = await CreateIngredientAsync();
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/ingredients/edit/{ingredient.Id}");
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Pepper", content);

        response = await client.GetAsync("/ingredients/edit/1000");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Edit_HandlesValidAndInvalidData()
    {
        Ingredient ingredient = await CreateIngredientAsync();
        var client = Factory.CreateClient();
        FormUrlEncodedContent formData = new(new Dictionary<string, string>
        {
            ["Id"] = ingredient.Id.ToString(),
            ["Name"] = "Black Pepper"
        });
        await client.PostAsync($"/ingredients/edit/{ingredient.Id}", formData);

        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Ingredient? updated = await dbContext.Ingredients.FindAsync(ingredient.Id);
        Assert.Equal("Black Pepper", updated!.Name);

        formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = ingredient.Id.ToString(),
            ["Name"] = ""
        });
        await client.PostAsync($"/ingredients/edit/{ingredient.Id}", formData);

        Ingredient? unchanged = await dbContext.Ingredients.FindAsync(ingredient.Id);
        Assert.Equal("Black Pepper", unchanged!.Name);
    }
}
