namespace RecipeMatcher.Web.Models.ViewModels;

public class MatchedRecipeViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int PreparationMinutes { get; set; }
    public IReadOnlyList<string> IngredientNames { get; set; } = [];
    public IReadOnlyList<string> MissingIngredients { get; set; } = [];
    public int MissingCount { get; set; }
}