using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RecipeMatcher.Web.Data;

namespace RecipeMatcher.Web.Tests;

// The same (repo, service , Domain....) MAAR new DB (DataSource=:memory:)
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
//  WebApplicationFactory >> I want to create trial version of the RecipeMatcher.Web 
// Program >> The entry point for an ASP.NET Core app....
{

    // this means the SQLite DB exists only in the compuster's RAM memory....
    private readonly SqliteConnection Connection = new("DataSource=:memory:");
    public CustomWebApplicationFactory() => Connection.Open();
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor is not null) services.Remove(descriptor); // I delete the old registration...
            services.AddDbContext<AppDbContext>(options => // When the AppDbContext app needs it,,,, git it theis DbContext that uses SQLite
            {
                options.UseSqlite(Connection);
            });
        });
    }

    // This means I am cleaning up the resources I have opened...
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Connection.Dispose();
    }
}