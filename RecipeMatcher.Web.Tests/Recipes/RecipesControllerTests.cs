using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Tests.Recipes;

public class RecipesControllerTests(CustomWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    [Fact]
    public async Task Index_ReturnsOkAndShowsRecipeName()
    {
        var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Recipes.Add(new Recipe
        {
            Name = "fishDish",
            PreparationMinutes = 30
        });
        await dbContext.SaveChangesAsync();
        var client = CreateClient();
        var response = await client.GetAsync("/recipes");
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("fishDish", content);
    }

    [Fact]
    public async Task Get_Create_ReturnsOkWithForm()
    {
        var client = Factory.CreateClient();
        var response = await client.GetAsync("/recipes/create");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Post_Create_WithValidData_RedirectsToIndex()
    {
        var client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var formData = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Name"] = "Omelette",
            ["PreparationMinutes"] = "10"
        });
        var response = await client.PostAsync("/recipes/create", formData);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

}