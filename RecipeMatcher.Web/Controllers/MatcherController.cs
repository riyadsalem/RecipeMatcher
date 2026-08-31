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
            .Where(recipe => recipe.RecipeIngredients
                .All(r => ingredientIds.Contains(r.IngredientId)))
            .OrderBy(recipe => recipe.Name)
            .Select(recipe => new MatchedRecipeViewModel
            {
                Id = recipe.Id,
                Name = recipe.Name,
                PreparationMinutes = recipe.PreparationMinutes,
                IngredientNames = recipe.RecipeIngredients
                    .Select(r => r.Ingredient.Name)
                    .ToList()
            })
            .ToListAsync();


}