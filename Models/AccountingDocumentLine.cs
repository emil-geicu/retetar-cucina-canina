using System.ComponentModel.DataAnnotations;

namespace CucinaCanina.Models;

public class AccountingDocumentLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountingDocumentId { get; set; }
    public AccountingDocument AccountingDocument { get; set; } = null!;

    public Guid? IngredientId { get; set; }
    public Ingredient? Ingredient { get; set; }

    [Required, StringLength(150)]
    public string Description { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string Unit { get; set; } = "kg";

    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total => Quantity * UnitPrice;
}
