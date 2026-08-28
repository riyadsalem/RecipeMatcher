using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Ingredients;

public class IngredientsControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
// Zoals (RecipesTests files)
{
    [Fact]
    public async Task Index_ReturnsOkAndShowsIngredientName()
    {
        var scope = factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureCreatedAsync();
        dbContext.Ingredients.Add(new Ingredient
        {
            Name = "Salt"
        });
        await dbContext.SaveChangesAsync();
        var client = factory.CreateClient();
        var response = await client.GetAsync("/ingredients");
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Salt", content);
    }
}
