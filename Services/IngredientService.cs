using CucinaCanina.Data;
using CucinaCanina.Models;
using CucinaCanina.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CucinaCanina.Services;

public class IngredientService(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public async Task<List<Ingredient>> GetAllIngredientsAsync()
    {
        using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Ingredients
            .Include(i => i.ReferenceIngredient)
            .OrderBy(i => i.Name)
            .ToListAsync();
    }

    public async Task<Ingredient?> GetIngredientByIdAsync(Guid id)
    {
        using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Ingredients
            .Include(i => i.ReferenceIngredient)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task AddIngredientAsync(Ingredient ingredient)
    {
        using var context = await dbContextFactory.CreateDbContextAsync();
        context.Ingredients.Add(ingredient);
        await context.SaveChangesAsync();
    }

    public async Task UpdateIngredientAsync(Ingredient ingredient)
    {
        using var context = await dbContextFactory.CreateDbContextAsync();
        context.Ingredients.Update(ingredient);
        await context.SaveChangesAsync();
    }

    public async Task DeleteIngredientAsync(Guid id)
    {
        using var context = await dbContextFactory.CreateDbContextAsync();
        var ingredient = await context.Ingredients.FindAsync(id);
        if (ingredient != null)
        {
            context.Ingredients.Remove(ingredient);
            await context.SaveChangesAsync();
        }
    }

    // Logic: Real Cost per Kg
    public decimal CalculateRealCostPerKg(Ingredient ingredient)
    {
        // Determine the base price: from Reference if CookedFood has one, otherwise the ingredient's own price
        decimal priceToUse = (ingredient.Category == IngredientCategory.CookedFood && ingredient.ReferenceIngredient != null)
            ? ingredient.ReferenceIngredient.BasePricePerKg
            : ingredient.BasePricePerKg;

        if (ingredient.Category == IngredientCategory.CookedFood)
        {
            return priceToUse * (ingredient.CookedMultiplier ?? 1.0m);
        }
        return priceToUse;
    }

    // Logic: Recommended Price per Kg
    public decimal CalculateRecommendedPricePerKg(Ingredient ingredient)
    {
        return CalculateRealCostPerKg(ingredient) * ingredient.RecommendedPriceMultiplier;
    }
}
