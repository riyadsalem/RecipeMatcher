using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests;

public abstract class IntegrationTestBase(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    protected CustomWebApplicationFactory Factory { get; } = factory;
    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
    public Task DisposeAsync() => Task.CompletedTask;
    protected HttpClient CreateClient() => Factory.CreateClient();
    protected HttpClient CreateClientWithoutRedirects() => Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
}