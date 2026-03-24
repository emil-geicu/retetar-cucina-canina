using CucinaCanina.Data;
using CucinaCanina.Models;
using CucinaCanina.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CucinaCanina.Services;

public class RecipeService(IDbContextFactory<AppDbContext> dbContextFactory, IngredientService ingredientService)
{
    public async Task<List<Recipe>> GetAllRecipesAsync()
    {
        using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Recipes
            .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
            .ThenInclude(i => i.ReferenceIngredient)
            .OrderBy(r => r.Name)
            .ToListAsync();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(Guid id)
    {
        using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Recipes
            .Include(r => r.RecipeIngredients)
                .ThenInclude(ri => ri.Ingredient)
                    .ThenInclude(i => i!.ReferenceIngredient)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task SaveRecipeAsync(Recipe recipe)
    {
        using var context = await dbContextFactory.CreateDbContextAsync();

        // Check if recipe already exists
        var existingRecipe = await context.Recipes
            .Include(r => r.RecipeIngredients)
            .FirstOrDefaultAsync(r => r.Id == recipe.Id);

        if (existingRecipe != null)
        {
            // Simple update: Clear existing ingredients and add new ones
            context.RecipeIngredients.RemoveRange(existingRecipe.RecipeIngredients);
            existingRecipe.Name = recipe.Name;
            existingRecipe.IdealPrice = recipe.IdealPrice;
            existingRecipe.RecipeIngredients = recipe.RecipeIngredients;
            context.Recipes.Update(existingRecipe);
        }
        else
        {
            context.Recipes.Add(recipe);
        }
        await context.SaveChangesAsync();
    }

    public async Task DeleteRecipeAsync(Guid id)
    {
        using var context = await dbContextFactory.CreateDbContextAsync();
        var recipe = await context.Recipes.FindAsync(id);
        if (recipe != null)
        {
            context.Recipes.Remove(recipe);
            await context.SaveChangesAsync();
        }
    }

    // Business Logic: Total Recommended Price for one portion
    public decimal CalculateTotalRecommendedPrice(Recipe recipe)
    {
        decimal total = 0;
        foreach (var ri in recipe.RecipeIngredients)
        {
            if (ri.Ingredient != null)
            {
                var recommendedPricePerGram = ingredientService.CalculateRecommendedPricePerKg(ri.Ingredient) / 1000m;
                total += recommendedPricePerGram * ri.QuantityGrams;
            }
        }
        return total;
    }

    // Business Logic: Profit Difference
    public decimal CalculateDifference(Recipe recipe)
    {
        return recipe.IdealPrice - CalculateTotalRecommendedPrice(recipe);
    }

    // Business Logic: Status Color based on Difference
    public string GetStatusColor(decimal difference)
    {
        if (difference > 0) return "#28a745"; // Success/Green
        if (difference == 0) return "#ffc107"; // Warning/Yellow
        return "#dc3545"; // Danger/Red
    }

    // Romanian translations for Status Color
    public string GetDifferenceStatusText(decimal difference)
    {
        if (difference > 0) return "PROFIT POZITIV";
        if (difference == 0) return "EGALITATE";
        return "PIERDERE / SUB PREȚ RECOMANDAT";
    }
}
