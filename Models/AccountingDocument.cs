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

    [StringLength(150)]
    public string? UnitName { get; set; }
    [StringLength(100)]
    public string? AccompanyingDocumentNumber { get; set; }
    [StringLength(100)]
    public string? InvoiceNumber { get; set; }
    [StringLength(200)]
    public string? ReceiptSource { get; set; }
    [StringLength(100)]
    public string? VehicleNumber { get; set; }
    [StringLength(500)]
    public string? AccompanyingDocuments { get; set; }
    [StringLength(150)]
    public string? DelegateName { get; set; }
    [StringLength(150)]
    public string? CommissionMemberOne { get; set; }
    [StringLength(150)]
    public string? WarehouseReceiverName { get; set; }

    public decimal VatRate { get; set; } = 11m;

    [StringLength(500)]
    public string? Notes { get; set; }

    public ICollection<AccountingDocumentLine> Lines { get; set; } = new List<AccountingDocumentLine>();
    public ICollection<AccountingDocumentAttachment> Attachments { get; set; } = new List<AccountingDocumentAttachment>();

    public decimal NetTotal => Lines.Sum(line => line.PurchaseValue);
    public decimal VatAmount => Lines.Sum(line => line.VatPaidValue);
    public decimal Total => Lines.Sum(line => line.VatTotalValue);
}
