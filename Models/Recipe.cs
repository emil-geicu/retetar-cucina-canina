using System.ComponentModel.DataAnnotations;

namespace CucinaCanina.Models;

public class Recipe
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    public decimal IdealPrice { get; set; }

    public ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
