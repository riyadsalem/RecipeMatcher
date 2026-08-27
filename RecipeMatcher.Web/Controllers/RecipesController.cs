using Microsoft.AspNetCore.Mvc;
using RecipeMatcher.Web.Models;
namespace RecipeMatcher.Web.Controllers;

public class RecipesController : Controller
{
    public IActionResult Index()
    {
        List<Recipe> recipes =
            [
                new() { Id = 1, Name = "Pancakes", PreparationMinutes = 20 },
                new() { Id = 2, Name = "Tomato Soup", PreparationMinutes = 30 }
            ];
        return View(recipes);
    }
}