# Recipe Matcher

A small ASP.NET Core MVC application. You select the ingredients you have
in your pantry, and it shows you which recipes you can make right now, and
which ones you're close to making.

```
Pantry: egg, milk, flour

Pancakes .......... You have everything.
Cake .............. Missing: butter
```

## Architecture

The app follows the MVC pattern:

- **Models** — `Recipe`, `Ingredient`, and `RecipeIngredient` (the join
  entity connecting them) represent the data. Screens that need more than
  one entity, like the recipe edit form with its ingredient checklist, are
  served by dedicated ViewModels (`EditRecipeViewModel`, `PantryViewModel`,
  `MatchedRecipeViewModel`) instead of exposing the entities directly.
- **Views** — Razor (`.cshtml`) templates, one folder per controller.
- **Controllers** — `RecipesController`, `IngredientsController`, and
  `MatcherController` handle requests, query the database through EF Core,
  and pass a Model or ViewModel to the View.

Routing uses ASP.NET Core's default convention
(`{controller}/{action}/{id?}`), so `/recipes/edit/5` maps to
`RecipesController.Edit(5)` without any extra configuration.

## Features

- Full CRUD for recipes (list, details, create, edit, delete) and
  ingredients (list, create, edit, delete)
- A many-to-many relationship between recipes and ingredients, modeled
  with an explicit join entity and a composite key
- Server-side validation (required fields, length limits, numeric ranges)
- A matcher page: pick the ingredients you have, and every recipe is
  ranked by how many ingredients it's missing, computed and sorted in a
  single database query

## Tech stack

ASP.NET Core MVC (.NET 8, C#), Entity Framework Core with SQLite, and
xUnit with `WebApplicationFactory` for integration tests.

## Project structure

```
RecipeMatcher.Web/
├── Controllers/       RecipesController, IngredientsController, MatcherController
├── Models/
│   ├── Recipe.cs
│   ├── Ingredient.cs
│   ├── RecipeIngredient.cs   join entity, composite key
│   └── ViewModels/
├── Views/              Recipes/, Ingredients/, Matcher/, Shared/
├── Data/                AppDbContext.cs
└── Migrations/          EF Core migrations

RecipeMatcher.Web.Tests/
├── CustomWebApplicationFactory.cs
├── IntegrationTestBase.cs    shared setup, resets the DB before each test
├── Recipes/
└── Ingredients/
```

## Getting started

Requires the .NET 8 SDK.

```bash
dotnet restore
dotnet ef database update --project RecipeMatcher.Web
dotnet run --project RecipeMatcher.Web
```

Visit `/recipes`, `/ingredients`, or `/matcher`.

## Running the tests

```bash
dotnet test
```

25 integration tests run against the real ASP.NET Core pipeline via
`WebApplicationFactory`. The database is reset before every test, so
results don't depend on the order tests run in.

## How matching works

For every recipe, the number of missing ingredients is calculated inside
the database query itself and used to sort the results — full matches
come first, followed by the closest partial matches.