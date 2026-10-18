using CucinaCanina.Models;
using Microsoft.EntityFrameworkCore;

namespace CucinaCanina.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<AccountingDocument> AccountingDocuments => Set<AccountingDocument>();
    public DbSet<AccountingDocumentLine> AccountingDocumentLines => Set<AccountingDocumentLine>();
    public DbSet<AccountingDocumentAttachment> AccountingDocumentAttachments => Set<AccountingDocumentAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Composite PK for many-to-many
        modelBuilder.Entity<RecipeIngredient>()
            .HasKey(ri => new { ri.RecipeId, ri.IngredientId });

        // Join table relationship for Recipes
        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Recipe)
            .WithMany(r => r.RecipeIngredients)
            .HasForeignKey(ri => ri.RecipeId);

        // Join table relationship for Ingredients
        modelBuilder.Entity<RecipeIngredient>()
            .HasOne(ri => ri.Ingredient)
            .WithMany(i => i.RecipeIngredients)
            .HasForeignKey(ri => ri.IngredientId);

        modelBuilder.Entity<AccountingDocumentLine>()
            .HasOne(line => line.AccountingDocument)
            .WithMany(document => document.Lines)
            .HasForeignKey(line => line.AccountingDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AccountingDocumentLine>()
            .HasOne(line => line.Ingredient)
            .WithMany()
            .HasForeignKey(line => line.IngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<AccountingDocumentAttachment>()
            .HasOne(attachment => attachment.AccountingDocument)
            .WithMany(document => document.Attachments)
            .HasForeignKey(attachment => attachment.AccountingDocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<AccountingDocumentAttachment>()
            .Property(attachment => attachment.FileData)
            .HasColumnType("varbinary(max)");

        // Self-referencing relationship for CookedFood referencing RawFood
        modelBuilder.Entity<Ingredient>()
            .HasOne(i => i.ReferenceIngredient)
            .WithMany()
            .HasForeignKey(i => i.ReferenceIngredientId)
            .OnDelete(DeleteBehavior.Restrict);

        // Standard Decimal Precision (18, 4) for financial calculations
        foreach (var property in modelBuilder.Model.GetEntityTypes()
            .SelectMany(t => t.GetProperties())
            .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetPrecision(18);
            property.SetScale(4);
        }
    }
}
