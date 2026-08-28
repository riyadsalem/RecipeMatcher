using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Ingredients;

public class IngredientsDeleteTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
// Zoals (RecipesTests files)
{
    private async Task<Ingredient> CreateIngredientAsync()
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        Ingredient ingredient = new()
        {
            Name = "Basil"
        };
        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync();
        return ingredient;
    }

    [Fact]
    public async Task Get_Delete_ReturnsCorrectResult()
    {
        Ingredient ingredient = await CreateIngredientAsync();
        var client = factory.CreateClient();

        var response = await client.GetAsync($"/ingredients/delete/{ingredient.Id}");
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Basil", content);

        response = await client.GetAsync("/ingredients/delete/1000");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Delete_ReturnsCorrectResult()
    {
        Ingredient ingredient = await CreateIngredientAsync();
        var client = factory.CreateClient();

        FormUrlEncodedContent formData = new(new Dictionary<string, string> { ["Id"] = ingredient.Id.ToString() });
        await client.PostAsync($"/ingredients/delete/{ingredient.Id}", formData);

        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Ingredient? deleted = await dbContext.Ingredients.FindAsync(ingredient.Id);
        Assert.Null(deleted);

        formData = new FormUrlEncodedContent(new Dictionary<string, string> { ["Id"] = "1000" });
        var response = await client.PostAsync("/ingredients/delete/1000", formData);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
