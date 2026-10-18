using CucinaCanina.Data;
using CucinaCanina.Models;
using CucinaCanina.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CucinaCanina.Services;

public class AccountingService(IDbContextFactory<AppDbContext> dbContextFactory)
{
    public async Task<List<AccountingDocument>> GetDocumentsAsync(AccountingDocumentType? type = null)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var query = context.AccountingDocuments
            .Include(document => document.Lines)
            .ThenInclude(line => line.Ingredient)
            .Include(document => document.Attachments)
            .AsNoTracking()
            .OrderByDescending(document => document.DocumentDate)
            .ThenByDescending(document => document.Number);

        return type.HasValue
            ? await query.Where(document => document.Type == type.Value).ToListAsync()
            : await query.ToListAsync();
    }

    public async Task<List<Ingredient>> GetIngredientsAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.Ingredients.AsNoTracking().OrderBy(i => i.Name).ToListAsync();
    }

    public async Task<AccountingDocument?> GetDocumentByIdAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.AccountingDocuments
            .Include(document => document.Lines)
            .ThenInclude(line => line.Ingredient)
            .Include(document => document.Attachments)
            .AsNoTracking()
            .FirstOrDefaultAsync(document => document.Id == id);
    }

    public async Task AddDocumentAsync(AccountingDocument document)
    {
        if (document.Lines.Count == 0)
            throw new InvalidOperationException("Adaugă cel puțin un articol documentului.");

        if (document.Lines.Any(line => line.Quantity <= 0))
            throw new InvalidOperationException("Cantitățile trebuie să fie mai mari decât zero.");

        await using var context = await dbContextFactory.CreateDbContextAsync();

        // Lines are built from the no-tracking ingredient list used by the UI.
        // Keep only the FK so EF does not try to insert the already existing ingredient.
        foreach (var line in document.Lines)
        {
            line.Ingredient = null;
        }

        context.AccountingDocuments.Add(document);
        await context.SaveChangesAsync();
    }

    public async Task UpdateDocumentAsync(AccountingDocument document)
    {
        if (document.Lines.Count == 0)
            throw new InvalidOperationException("Adaugă cel puțin un articol documentului.");

        if (document.Lines.Any(line => line.Quantity <= 0))
            throw new InvalidOperationException("Cantitățile trebuie să fie mai mari decât zero.");

        await using var context = await dbContextFactory.CreateDbContextAsync();
        var existing = await context.AccountingDocuments
            .Include(item => item.Lines)
            .Include(item => item.Attachments)
            .FirstOrDefaultAsync(item => item.Id == document.Id);

        if (existing is null)
            throw new InvalidOperationException("Documentul nu mai există.");

        existing.Number = document.Number;
        existing.DocumentDate = document.DocumentDate;
        existing.PartnerName = document.PartnerName;
        existing.Type = document.Type;
        existing.VatRate = document.VatRate;
        existing.Notes = document.Notes;

        context.AccountingDocumentLines.RemoveRange(existing.Lines);
        await context.SaveChangesAsync();

        existing.Lines.Clear();
        foreach (var line in document.Lines)
        {
            existing.Lines.Add(new AccountingDocumentLine
            {
                Id = Guid.NewGuid(),
                AccountingDocumentId = existing.Id,
                IngredientId = line.IngredientId,
                Description = line.Description,
                Unit = line.Unit,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice
            });
        }

        foreach (var attachment in document.Attachments.Where(attachment =>
            !existing.Attachments.Any(existingAttachment => existingAttachment.Id == attachment.Id)))
        {
            existing.Attachments.Add(new AccountingDocumentAttachment
            {
                Id = attachment.Id == Guid.Empty ? Guid.NewGuid() : attachment.Id,
                AccountingDocumentId = existing.Id,
                OriginalFileName = attachment.OriginalFileName,
                StoredFileName = attachment.StoredFileName,
                ContentType = attachment.ContentType,
                FileData = attachment.FileData,
                UploadedAt = attachment.UploadedAt
            });
        }

        await context.SaveChangesAsync();
    }

    public async Task DeleteDocumentAsync(Guid id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var document = await context.AccountingDocuments.FindAsync(id);
        if (document is null) return;
        context.AccountingDocuments.Remove(document);
        await context.SaveChangesAsync();
    }

    public async Task<(decimal Entries, decimal Exits)> GetStockSummaryAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var entries = await context.AccountingDocumentLines
            .Where(line => line.AccountingDocument.Type == AccountingDocumentType.ReceptionNote)
            .SumAsync(line => (decimal?)line.Quantity) ?? 0;
        var exits = await context.AccountingDocumentLines
            .Where(line => line.AccountingDocument.Type == AccountingDocumentType.ConsumptionNote)
            .SumAsync(line => (decimal?)line.Quantity) ?? 0;
        return (entries, exits);
    }
}
