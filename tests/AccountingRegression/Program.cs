using CucinaCanina.Data;
using CucinaCanina.Models;
using CucinaCanina.Models.Enums;
using CucinaCanina.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

// Run with: dotnet run --project tests/AccountingRegression
using var connection = new SqliteConnection("Data Source=:memory:");
await connection.OpenAsync();
var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
var service = new AccountingService(new ContextFactory(options));
await using (var context = new SqliteTestContext(options))
{
    await context.Database.EnsureCreatedAsync();
}

var attachment = new AccountingDocumentAttachment
{
    OriginalFileName = "invoice.pdf", StoredFileName = "invoice.pdf",
    ContentType = "application/pdf", FileData = [1, 2, 3]
};
var document = new AccountingDocument
{
    Number = "12345", PartnerName = "Supplier", Type = AccountingDocumentType.ReceptionNote,
    Lines = [Line("Original", 2)], Attachments = [attachment]
};
await service.AddDocumentAsync(document);
var originalLineId = document.Lines.Single().Id;
var edit = (await service.GetDocumentByIdAsync(document.Id))!;
edit.Number = "Edited";
edit.Lines = [Line("Replacement", 3), Line("Additional", 4)];
var newAttachment = new AccountingDocumentAttachment
{
    OriginalFileName = "second.pdf", StoredFileName = "second.pdf", FileData = [4, 5]
};
edit.Attachments.Add(newAttachment);
await service.UpdateDocumentAsync(edit);
var saved = (await service.GetDocumentByIdAsync(document.Id))!;
Check(saved.Number == "Edited" && saved.Lines.Count == 2, "Edit persists header and replacement lines");
Check(saved.Lines.All(line => line.Id != originalLineId), "Old lines are removed");
Check(saved.Lines.Single(line => line.Description == "Replacement").Quantity == 3, "Quantity persists");
Check(saved.Attachments.Count == 2 && saved.Attachments.Any(a => a.Id == newAttachment.Id), "New attachment is inserted");
Check(saved.Attachments.Single(a => a.Id == attachment.Id).FileData!.SequenceEqual(new byte[] { 1, 2, 3 }), "Existing attachment is preserved");

// Repeat the edit with the same detached input; attachments must not be duplicated.
await service.UpdateDocumentAsync(edit);
saved = (await service.GetDocumentByIdAsync(document.Id))!;
Check(saved.Lines.Count == 2 && saved.Attachments.Count == 2, "Repeated edits succeed without duplicates");

// A real relational FK failure must roll back the header change and line deletion.
var savedLineIds = saved.Lines.Select(line => line.Id).Order().ToArray();
edit.Number = "Must roll back";
edit.Lines = [Line("Invalid ingredient", 1)];
edit.Lines.Single().IngredientId = Guid.NewGuid();
try
{
    await service.UpdateDocumentAsync(edit);
    throw new Exception("Expected foreign-key failure");
}
catch (DbUpdateException) { }
saved = (await service.GetDocumentByIdAsync(document.Id))!;
Check(saved.Number == "Edited", "Failed edit rolls back header");
Check(saved.Lines.Select(line => line.Id).Order().SequenceEqual(savedLineIds), "Failed edit preserves original lines");
Check(saved.Attachments.Count == 2, "Failed edit preserves attachments");

// Documents emptied by the old two-save implementation can be repaired by editing.
await using (var context = new SqliteTestContext(options))
{
    context.AccountingDocumentLines.RemoveRange(await context.AccountingDocumentLines.ToListAsync());
    await context.SaveChangesAsync();
}
edit.Lines = [Line("Recovered", 5)];
await service.UpdateDocumentAsync(edit);
saved = (await service.GetDocumentByIdAsync(document.Id))!;
Check(saved.Lines.Single().Description == "Recovered", "Editing repairs a document with no stored lines");
Console.WriteLine("All accounting regression checks passed.");

static AccountingDocumentLine Line(string description, decimal quantity) => new()
{
    Description = description, Quantity = quantity, UnitPrice = 10,
    PurchaseUnitPrice = 10, PurchaseValue = quantity * 10,
    SaleTotalValue = quantity * 12, VatRate = 11,
    VatPaidValue = quantity * 1.1m, VatTotalValue = quantity * 11.1m
};

static void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine($"PASS: {message}");
}

sealed class ContextFactory(DbContextOptions<AppDbContext> options) : IDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext() => new SqliteTestContext(options);
}

// Adapt only the SQL Server-specific binary column type for the test database.
sealed class SqliteTestContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AccountingDocumentAttachment>()
            .Property(attachment => attachment.FileData).HasColumnType("BLOB");
    }
}
