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
        Recipe? recipe = await dbContext.Recipes.FindAsync(id);
        return recipe is null ? NotFound() : View(recipe);
    }


}