using System.ComponentModel.DataAnnotations;

namespace CucinaCanina.Models;

public class RecipeIngredient
{
    public Guid RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    public Guid IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }

    public decimal QuantityGrams { get; set; }
}
