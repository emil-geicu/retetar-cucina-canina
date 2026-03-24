using CucinaCanina.Models;
using CucinaCanina.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CucinaCanina.Data;

public static class DbInitializer
{
    public static async Task Initialize(IDbContextFactory<AppDbContext> dbContextFactory)
    {
        using var context = await dbContextFactory.CreateDbContextAsync();

        // ENSURE DATABASE CREATED
        await context.Database.EnsureCreatedAsync();

        // Skip if already has data
        if (context.Ingredients.Any()) return;

        // 1. Raw Foods
        var puiCrud = new Ingredient { Name = "Pui Crud", Category = IngredientCategory.RawFood, BasePricePerKg = 25.00m, RecommendedPriceMultiplier = 2.0m };
        var vitaCruda = new Ingredient { Name = "Vită Crudă", Category = IngredientCategory.RawFood, BasePricePerKg = 45.00m, RecommendedPriceMultiplier = 2.0m };
        var orezRaw = new Ingredient { Name = "Orez (Raw)", Category = IngredientCategory.RawFood, BasePricePerKg = 8.50m, RecommendedPriceMultiplier = 2.0m };
        var morcovRaw = new Ingredient { Name = "Morcov Crud", Category = IngredientCategory.RawFood, BasePricePerKg = 5.00m, RecommendedPriceMultiplier = 2.0m };

        context.Ingredients.AddRange(puiCrud, vitaCruda, orezRaw, morcovRaw);
        await context.SaveChangesAsync();

        // 2. Cooked Foods
        var puiGatit = new Ingredient { 
            Name = "Pui Gătit", 
            Category = IngredientCategory.CookedFood, 
            ReferenceIngredientId = puiCrud.Id, 
            CookedMultiplier = 1.35m, // 1kg pui gatit = 1.35kg crud
            RecommendedPriceMultiplier = 2.0m 
        };
        
        var orezGatit = new Ingredient { 
            Name = "Orez Gătit", 
            Category = IngredientCategory.CookedFood, 
            ReferenceIngredientId = orezRaw.Id, 
            CookedMultiplier = 2.80m, // 1kg orez gatit = 2.8kg de orez crud (se umfla la fierbere)
            RecommendedPriceMultiplier = 2.0m 
        };

        context.Ingredients.AddRange(puiGatit, orezGatit);
        await context.SaveChangesAsync();

        // 3. Vitamins
        var komplexVitamins = new Ingredient { 
            Name = "Complex Vitamine Supreme", 
            Category = IngredientCategory.Vitamins, 
            BasePricePerKg = 180.00m, 
            RecommendedPriceMultiplier = 1.2m 
        };
        context.Ingredients.Add(komplexVitamins);
        await context.SaveChangesAsync();

        // 4. Recipes
        var recipe1 = new Recipe { Name = "Porție Pui cu Orez Standard", IdealPrice = 18.50m };
        recipe1.RecipeIngredients.Add(new RecipeIngredient { IngredientId = puiGatit.Id, QuantityGrams = 180m });
        recipe1.RecipeIngredients.Add(new RecipeIngredient { IngredientId = orezGatit.Id, QuantityGrams = 120m });
        recipe1.RecipeIngredients.Add(new RecipeIngredient { IngredientId = morcovRaw.Id, QuantityGrams = 40m });
        recipe1.RecipeIngredients.Add(new RecipeIngredient { IngredientId = komplexVitamins.Id, QuantityGrams = 8m });

        var recipe2 = new Recipe { Name = "Vită Deluxe (Grain Free)", IdealPrice = 35.00m };
        recipe2.RecipeIngredients.Add(new RecipeIngredient { IngredientId = vitaCruda.Id, QuantityGrams = 250m });
        recipe2.RecipeIngredients.Add(new RecipeIngredient { IngredientId = morcovRaw.Id, QuantityGrams = 100m });
        recipe2.RecipeIngredients.Add(new RecipeIngredient { IngredientId = komplexVitamins.Id, QuantityGrams = 12m });

        context.Recipes.AddRange(recipe1, recipe2);
        await context.SaveChangesAsync();
    }
}
