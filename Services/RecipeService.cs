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
            existingRecipe.UseCustomProductionFactor = recipe.UseCustomProductionFactor;
            existingRecipe.CustomProductionFactor = recipe.UseCustomProductionFactor
                ? GetEffectiveCustomFactor(recipe)
                : null;
            existingRecipe.RecipeIngredients = recipe.RecipeIngredients;
            context.Recipes.Update(existingRecipe);
        }
        else
        {
            if (recipe.UseCustomProductionFactor)
            {
                recipe.CustomProductionFactor = GetEffectiveCustomFactor(recipe);
            }
            else
            {
                recipe.CustomProductionFactor = null;
            }
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
        if (recipe.UseCustomProductionFactor)
        {
            return CalculateTotalRealPrice(recipe) * GetEffectiveCustomFactor(recipe);
        }

        return recipe.RecipeIngredients.Sum(CalculateDefaultRecommendedRowPrice);
    }

    public decimal CalculateTotalRealPrice(Recipe recipe)
    {
        return recipe.RecipeIngredients.Sum(CalculateRealRowPrice);
    }

    public decimal CalculateRowRecommendedPrice(Recipe recipe, RecipeIngredient recipeIngredient)
    {
        if (recipe.UseCustomProductionFactor)
        {
            return CalculateRealRowPrice(recipeIngredient) * GetEffectiveCustomFactor(recipe);
        }

        return CalculateDefaultRecommendedRowPrice(recipeIngredient);
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

    private decimal CalculateRealRowPrice(RecipeIngredient recipeIngredient)
    {
        if (recipeIngredient.Ingredient == null) return 0;

        var realPricePerGram = ingredientService.CalculateRealCostPerKg(recipeIngredient.Ingredient) / 1000m;
        return realPricePerGram * recipeIngredient.QuantityGrams;
    }

    private decimal CalculateDefaultRecommendedRowPrice(RecipeIngredient recipeIngredient)
    {
        if (recipeIngredient.Ingredient == null) return 0;

        var recommendedPricePerGram = ingredientService.CalculateRecommendedPricePerKg(recipeIngredient.Ingredient) / 1000m;
        return recommendedPricePerGram * recipeIngredient.QuantityGrams;
    }

    private static decimal GetEffectiveCustomFactor(Recipe recipe)
    {
        if (recipe.CustomProductionFactor.HasValue && recipe.CustomProductionFactor.Value > 0)
        {
            return recipe.CustomProductionFactor.Value;
        }

        return 1.0m;
    }
}
