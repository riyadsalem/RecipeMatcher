using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>()
        .HasIndex(ingredient => ingredient.Name).IsUnique();
        /*
        Important point:
        [Required] >> Only checks if the value is not empty in the same request.
        IsUnique() >> is a db level safeguard that prevents duplication even if two requests arrive simultaneously.
        */
    }
}