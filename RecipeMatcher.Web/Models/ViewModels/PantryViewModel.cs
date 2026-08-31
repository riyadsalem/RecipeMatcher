namespace RecipeMatcher.Web.Models.ViewModels;

public class PantryViewModel
{
    public IReadOnlyList<IngredientOptionViewModel> Ingredients { get; set; } = [];
    public IReadOnlyList<int> SelectedIngredientIds { get; set; } = [];
    public IReadOnlyList<MatchedRecipeViewModel> MatchedRecipes { get; set; } = [];
}