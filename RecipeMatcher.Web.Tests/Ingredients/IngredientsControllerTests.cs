using System.Net;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Ingredients;

public class IngredientsControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
// Zoals (RecipesTests files)
{
    [Fact]
    public async Task Index_ReturnsOkAndShowsIngredientName()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Ingredients.Add(new Ingredient
        {
            Name = "Salt"
        });
        await dbContext.SaveChangesAsync();
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/ingredients");
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Salt", content);
    }
}
