namespace CucinaCanina.Models;

public class AccountingDocumentAttachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AccountingDocumentId { get; set; }
    public AccountingDocument AccountingDocument { get; set; } = null!;
    public string OriginalFileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[]? FileData { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
