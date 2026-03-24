using CucinaCanina.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CucinaCanina.Models;

public class Ingredient
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public IngredientCategory Category { get; set; }

    public decimal BasePricePerKg { get; set; }

    public decimal RecommendedPriceMultiplier { get; set; }

    // Used for CookedFood referencing RawFood
    public Guid? ReferenceIngredientId { get; set; }
    public Ingredient? ReferenceIngredient { get; set; }

    // Multiplier for weight: 1kg cooked = CookedMultiplier * 1kg raw
    public decimal? CookedMultiplier { get; set; }

    // Many-to-many relationship helper
    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
