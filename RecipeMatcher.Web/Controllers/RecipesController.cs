using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
namespace RecipeMatcher.Web.Controllers;

public class RecipesController(AppDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        List<Recipe> recipes = await dbContext.Recipes.OrderBy(recipe => recipe.Name).ToListAsync();
        return View(recipes);
    }

    [HttpGet]
    public IActionResult Create() => View();


    [HttpPost]
    public async Task<IActionResult> Create(Recipe recipe)
    {
        if (!ModelState.IsValid) return View(recipe);
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        Recipe? recipe = await dbContext.Recipes
            .Include(recipe => recipe.RecipeIngredients)
            .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .FirstOrDefaultAsync(recipe => recipe.Id == id);
        return recipe is null ? NotFound() : View(recipe);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        Recipe? recipe = await dbContext.Recipes.FindAsync(id);
        return recipe is null ? NotFound() : View(recipe);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Recipe recipe)
    {
        if (id != recipe.Id) return NotFound();
        if (!ModelState.IsValid) return View(recipe);

        Recipe? existingRecipe = await dbContext.Recipes.FindAsync(id);
        if (existingRecipe is null) return NotFound();

        existingRecipe.Name = recipe.Name;
        existingRecipe.PreparationMinutes = recipe.PreparationMinutes;
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        Recipe? recipe = await dbContext.Recipes.FindAsync(id);
        return recipe is null ? NotFound() : View(recipe);
    }

    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        Recipe? recipe = await dbContext.Recipes.FindAsync(id);
        if (recipe is null) return NotFound();
        dbContext.Recipes.Remove(recipe);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }


}