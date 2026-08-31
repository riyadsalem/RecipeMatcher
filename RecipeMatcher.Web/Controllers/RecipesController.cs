using System.ComponentModel.Design;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;
using RecipeMatcher.Web.Models.ViewModels;
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
        Recipe? recipe = await dbContext.Recipes
            .Include(recipe => recipe.RecipeIngredients)
            .FirstOrDefaultAsync(recipe => recipe.Id == id);

        if (recipe is null) return NotFound();

        IEnumerable<int> selectedIngredientIds = recipe.RecipeIngredients
            .Select(recipeIngredient => recipeIngredient.IngredientId);

        EditRecipeViewModel viewModel = new()
        {
            Id = recipe.Id,
            Name = recipe.Name,
            PreparationMinutes = recipe.PreparationMinutes,
            Ingredients = await BuildIngredientOptionsAsync(selectedIngredientIds)
        };
        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, EditRecipeViewModel viewModel, int[]? ingredientIds)
    {
        ingredientIds ??= [];

        if (id != viewModel.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            viewModel.Ingredients = await BuildIngredientOptionsAsync(ingredientIds);
            return View(viewModel);
        }

        Recipe? existingRecipe = await dbContext.Recipes
            .Include(recipe => recipe.RecipeIngredients)
            .FirstOrDefaultAsync(recipe => recipe.Id == id);

        if (existingRecipe is null) return NotFound();

        existingRecipe.Name = viewModel.Name;
        existingRecipe.PreparationMinutes = viewModel.PreparationMinutes;

        existingRecipe.RecipeIngredients.Clear();
        foreach (int ingredientId in ingredientIds)
        {
            existingRecipe.RecipeIngredients.Add(new RecipeIngredient
            {
                RecipeId = existingRecipe.Id,
                IngredientId = ingredientId
            });
        }

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

    private async Task<IReadOnlyList<IngredientOptionViewModel>> BuildIngredientOptionsAsync(IEnumerable<int> selectedIngredientIds)
    => await dbContext.Ingredients
            .OrderBy(ingredient => ingredient.Name)
            .Select(ingredient => new IngredientOptionViewModel
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Selected = selectedIngredientIds.ToHashSet().Contains(ingredient.Id)
                // Convert to a HashSet to make repeated Contains checks faster....
            })
            .ToListAsync();



}