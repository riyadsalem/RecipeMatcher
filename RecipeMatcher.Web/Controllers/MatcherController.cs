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
    public async Task<IActionResult> Index(int[]? ingredientIds) =>
    View(await BuildPantryViewModelAsync(ingredientIds ?? []));

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
            SelectedIngredientIds = selectedSet.ToList()
        };
    }
}