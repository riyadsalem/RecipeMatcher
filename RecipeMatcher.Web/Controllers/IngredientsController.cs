using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecipeMatcher.Web.Data;
using RecipeMatcher.Web.Models;

namespace RecipeMatcher.Web.Controllers;


public class IngredientsController(AppDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index() =>
     View(await dbContext.Ingredients
     .OrderBy(ingredient => ingredient.Name)
     .ToListAsync());

    [HttpGet]
    public IActionResult Create() => View();

    [HttpPost]
    public async Task<IActionResult> Create(Ingredient ingredient)
    {
        bool nameExists = await dbContext.Ingredients
            .AnyAsync(existing => existing.Name == ingredient.Name);
        if (nameExists)
            ModelState.AddModelError(nameof(Ingredient.Name), "This ingredient already exists.");

        if (!ModelState.IsValid) return View(ingredient);

        dbContext.Ingredients.Add(ingredient);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        Ingredient? ingredient = await dbContext.Ingredients.FindAsync(id);
        return ingredient is null ? NotFound() : View(ingredient);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Ingredient ingredient)
    {
        if (id != ingredient.Id) return NotFound();

        bool nameExists = await dbContext.Ingredients
            .AnyAsync(existing => existing.Name == ingredient.Name && existing.Id != id);
        if (nameExists)
            ModelState.AddModelError(nameof(Ingredient.Name), "This ingredient already exists.");

        if (!ModelState.IsValid) return View(ingredient);

        Ingredient? existingIngredient = await dbContext.Ingredients.FindAsync(id);
        existingIngredient!.Name = ingredient.Name;
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        Ingredient? ingredient = await dbContext.Ingredients.FindAsync(id);
        return ingredient is null ? NotFound() : View(ingredient);
    }

    [HttpPost]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        Ingredient? ingredient = await dbContext.Ingredients.FindAsync(id);
        if (ingredient is null) return NotFound();
        dbContext.Ingredients.Remove(ingredient);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
