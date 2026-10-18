using System.ComponentModel.DataAnnotations;
using CucinaCanina.Models.Enums;

namespace CucinaCanina.Models;

public class AccountingDocument
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, StringLength(50)]
    public string Number { get; set; } = string.Empty;

    public DateTime DocumentDate { get; set; } = DateTime.Today;

    [Required, StringLength(150)]
    public string PartnerName { get; set; } = string.Empty;

    public AccountingDocumentType Type { get; set; }

    public decimal VatRate { get; set; } = 21m;

    [StringLength(500)]
    public string? Notes { get; set; }

    public ICollection<AccountingDocumentLine> Lines { get; set; } = new List<AccountingDocumentLine>();
    public ICollection<AccountingDocumentAttachment> Attachments { get; set; } = new List<AccountingDocumentAttachment>();

    public decimal NetTotal => Lines.Sum(line => line.Quantity * line.UnitPrice);
    public decimal VatAmount => NetTotal * VatRate / 100m;
    public decimal Total => NetTotal + VatAmount;
}
