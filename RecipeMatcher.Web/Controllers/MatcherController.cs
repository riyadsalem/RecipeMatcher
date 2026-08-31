using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models.ViewModels;

namespace RecipeMatcher.Web.Controllers;

public class MatcherController(AppDbContext dbContext) : Controller
{

    [HttpGet]
    public async Task<IActionResult> Index() =>
    View(await BuildPantryViewModelAsync([]));
    [HttpPost]
    public async Task<IActionResult> Index(int[]? ingredientIds)
    {
        ingredientIds ??= [];
        PantryViewModel viewModel = await BuildPantryViewModelAsync(ingredientIds);
        viewModel.MatchedRecipes = await FindMatchingRecipesAsync(ingredientIds);
        return View(viewModel);
    }
    private async Task<PantryViewModel> BuildPantryViewModelAsync(IEnumerable<int> selectedIngredientIds)
    {
        HashSet<int> selectedSet = selectedIngredientIds.ToHashSet();
        List<IngredientOptionViewModel> ingredients = await dbContext.Ingredients
            .OrderBy(ingredient => ingredient.Name)
            .Select(ingredient => new IngredientOptionViewModel
            {
                Id = ingredient.Id,
                Name = ingredient.Name,
                Selected = selectedSet.Contains(ingredient.Id)
            })
            .ToListAsync();

        return new PantryViewModel
        {
            Ingredients = ingredients,
            SelectedIngredientIds = [.. selectedSet]
        };
    }

    private async Task<IReadOnlyList<MatchedRecipeViewModel>> FindMatchingRecipesAsync(int[] ingredientIds)
    => await dbContext.Recipes
            .Select(recipe => new MatchedRecipeViewModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                PreparationMinutes = recipe.PreparationMinutes,

                IngredientNames = recipe.RecipeIngredients
                    .Select(ri => ri.Ingredient.Name)
                    .ToList(),

                MissingIngredients = recipe.RecipeIngredients
                    .Where(ri => !ingredientIds.Contains(ri.IngredientId))
                    .Select(ri => ri.Ingredient.Name)
                    .ToList(),

                MissingCount = recipe.RecipeIngredients
                    .Count(ri => !ingredientIds.Contains(ri.IngredientId))
            })
            .OrderBy(recipe => recipe.MissingCount)
            .ThenBy(recipe => recipe.Name)
            .ToListAsync();
}