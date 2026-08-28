using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ingredient>()
        .HasIndex(ingredient => ingredient.Name).IsUnique();
        /*
        Important point:
        [Required] >> Only checks if the value is not empty in the same request.
        IsUnique() >> is a db level safeguard that prevents duplication even if two requests arrive simultaneously.
        */

        modelBuilder.Entity<RecipeIngredient>()
            .HasKey(recipeIngredient => new { recipeIngredient.RecipeId, recipeIngredient.IngredientId }); // Composite Primary Key

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(recipeIngredient => recipeIngredient.Recipe)
            .WithMany(recipe => recipe.RecipeIngredients)
            .HasForeignKey(recipeIngredient => recipeIngredient.RecipeId);

        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(recipeIngredient => recipeIngredient.Ingredient)
            .WithMany(ingredient => ingredient.RecipeIngredients)
            .HasForeignKey(recipeIngredient => recipeIngredient.IngredientId);
    }
}