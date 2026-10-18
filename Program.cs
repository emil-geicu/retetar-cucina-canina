using CucinaCanina.Data;
using CucinaCanina.Services;
using Microsoft.EntityFrameworkCore;
using CucinaCanina.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IngredientService>();
builder.Services.AddScoped<RecipeService>();
builder.Services.AddScoped<AccountingService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>();
    await using var dbContext = await dbContextFactory.CreateDbContextAsync();
    await dbContext.Database.ExecuteSqlRawAsync("""
        IF OBJECT_ID(N'[Recipes]', N'U') IS NOT NULL
        BEGIN
            IF COL_LENGTH('Recipes', 'UseCustomProductionFactor') IS NULL
            BEGIN
                ALTER TABLE [Recipes] ADD [UseCustomProductionFactor] bit NOT NULL
                    CONSTRAINT [DF_Recipes_UseCustomProductionFactor] DEFAULT(0);
            END

            IF COL_LENGTH('Recipes', 'CustomProductionFactor') IS NULL
            BEGIN
                ALTER TABLE [Recipes] ADD [CustomProductionFactor] decimal(18,4) NULL;
            END
        END
        """);

    await dbContext.Database.ExecuteSqlRawAsync("""
        IF OBJECT_ID(N'[AccountingDocuments]', N'U') IS NULL
        BEGIN
            CREATE TABLE [AccountingDocuments] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_AccountingDocuments] PRIMARY KEY,
                [Number] nvarchar(50) NOT NULL,
                [DocumentDate] datetime2 NOT NULL,
                [PartnerName] nvarchar(150) NOT NULL,
                [Type] int NOT NULL,
                [Notes] nvarchar(500) NULL
            );
        END

        IF COL_LENGTH('AccountingDocuments', 'VatRate') IS NULL
        BEGIN
            ALTER TABLE [AccountingDocuments] ADD [VatRate] decimal(18,4) NOT NULL
                CONSTRAINT [DF_AccountingDocuments_VatRate] DEFAULT(11);
        END

        IF OBJECT_ID(N'[AccountingDocuments]', N'U') IS NOT NULL
        BEGIN
            IF COL_LENGTH('AccountingDocuments', 'UnitName') IS NULL ALTER TABLE [AccountingDocuments] ADD [UnitName] nvarchar(150) NULL;
            IF COL_LENGTH('AccountingDocuments', 'AccompanyingDocumentNumber') IS NULL ALTER TABLE [AccountingDocuments] ADD [AccompanyingDocumentNumber] nvarchar(100) NULL;
            IF COL_LENGTH('AccountingDocuments', 'InvoiceNumber') IS NULL ALTER TABLE [AccountingDocuments] ADD [InvoiceNumber] nvarchar(100) NULL;
            IF COL_LENGTH('AccountingDocuments', 'ReceiptSource') IS NULL ALTER TABLE [AccountingDocuments] ADD [ReceiptSource] nvarchar(200) NULL;
            IF COL_LENGTH('AccountingDocuments', 'VehicleNumber') IS NULL ALTER TABLE [AccountingDocuments] ADD [VehicleNumber] nvarchar(100) NULL;
            IF COL_LENGTH('AccountingDocuments', 'AccompanyingDocuments') IS NULL ALTER TABLE [AccountingDocuments] ADD [AccompanyingDocuments] nvarchar(500) NULL;
            IF COL_LENGTH('AccountingDocuments', 'DelegateName') IS NULL ALTER TABLE [AccountingDocuments] ADD [DelegateName] nvarchar(150) NULL;
            IF COL_LENGTH('AccountingDocuments', 'CommissionMemberOne') IS NULL ALTER TABLE [AccountingDocuments] ADD [CommissionMemberOne] nvarchar(150) NULL;
            IF COL_LENGTH('AccountingDocuments', 'WarehouseReceiverName') IS NULL ALTER TABLE [AccountingDocuments] ADD [WarehouseReceiverName] nvarchar(150) NULL;
        END

        IF OBJECT_ID(N'[AccountingDocumentLines]', N'U') IS NULL
        BEGIN
            CREATE TABLE [AccountingDocumentLines] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_AccountingDocumentLines] PRIMARY KEY,
                [AccountingDocumentId] uniqueidentifier NOT NULL,
                [IngredientId] uniqueidentifier NULL,
                [Description] nvarchar(150) NOT NULL,
                [Unit] nvarchar(20) NOT NULL,
                [Quantity] decimal(18,4) NOT NULL,
                [UnitPrice] decimal(18,4) NOT NULL,
                [PurchaseUnitPrice] decimal(18,4) NOT NULL DEFAULT(0),
                [PurchaseValue] decimal(18,4) NOT NULL DEFAULT(0),
                [CommercialMarkupUnitValue] decimal(18,4) NOT NULL DEFAULT(0),
                [CommercialMarkupTotalValue] decimal(18,4) NOT NULL DEFAULT(0),
                [SaleUnitPrice] decimal(18,4) NOT NULL DEFAULT(0),
                [SaleTotalValue] decimal(18,4) NOT NULL DEFAULT(0),
                [VatPaidValue] decimal(18,4) NOT NULL DEFAULT(0),
                [VatTotalValue] decimal(18,4) NOT NULL DEFAULT(0),
                [VatRate] decimal(18,4) NOT NULL DEFAULT(11),
                CONSTRAINT [FK_AccountingDocumentLines_AccountingDocuments_AccountingDocumentId]
                    FOREIGN KEY ([AccountingDocumentId]) REFERENCES [AccountingDocuments]([Id]) ON DELETE CASCADE,
                CONSTRAINT [FK_AccountingDocumentLines_Ingredients_IngredientId]
                    FOREIGN KEY ([IngredientId]) REFERENCES [Ingredients]([Id])
            );
        END

        IF OBJECT_ID(N'[AccountingDocumentAttachments]', N'U') IS NULL
        BEGIN
            CREATE TABLE [AccountingDocumentAttachments] (
                [Id] uniqueidentifier NOT NULL CONSTRAINT [PK_AccountingDocumentAttachments] PRIMARY KEY,
                [AccountingDocumentId] uniqueidentifier NOT NULL,
                [OriginalFileName] nvarchar(260) NOT NULL,
                [StoredFileName] nvarchar(260) NOT NULL,
                [ContentType] nvarchar(150) NOT NULL,
                [FileData] varbinary(max) NULL,
                [UploadedAt] datetime2 NOT NULL,
                CONSTRAINT [FK_AccountingDocumentAttachments_AccountingDocuments_AccountingDocumentId]
                    FOREIGN KEY ([AccountingDocumentId]) REFERENCES [AccountingDocuments]([Id]) ON DELETE CASCADE
            );
        END

        IF OBJECT_ID(N'[AccountingDocumentLines]', N'U') IS NOT NULL
        BEGIN
            IF COL_LENGTH('AccountingDocumentLines', 'PurchaseUnitPrice') IS NULL ALTER TABLE [AccountingDocumentLines] ADD [PurchaseUnitPrice] decimal(18,4) NOT NULL CONSTRAINT [DF_Lines_PurchaseUnitPrice] DEFAULT(0);
            IF COL_LENGTH('AccountingDocumentLines', 'PurchaseValue') IS NULL ALTER TABLE [AccountingDocumentLines] ADD [PurchaseValue] decimal(18,4) NOT NULL CONSTRAINT [DF_Lines_PurchaseValue] DEFAULT(0);
            IF COL_LENGTH('AccountingDocumentLines', 'SaleTotalValue') IS NULL ALTER TABLE [AccountingDocumentLines] ADD [SaleTotalValue] decimal(18,4) NOT NULL CONSTRAINT [DF_Lines_SaleTotal] DEFAULT(0);
            IF COL_LENGTH('AccountingDocumentLines', 'VatPaidValue') IS NULL ALTER TABLE [AccountingDocumentLines] ADD [VatPaidValue] decimal(18,4) NOT NULL CONSTRAINT [DF_Lines_VatPaid] DEFAULT(0);
            IF COL_LENGTH('AccountingDocumentLines', 'VatTotalValue') IS NULL ALTER TABLE [AccountingDocumentLines] ADD [VatTotalValue] decimal(18,4) NOT NULL CONSTRAINT [DF_Lines_VatTotal] DEFAULT(0);
            IF COL_LENGTH('AccountingDocumentLines', 'VatRate') IS NULL ALTER TABLE [AccountingDocumentLines] ADD [VatRate] decimal(18,4) NOT NULL CONSTRAINT [DF_Lines_VatRate] DEFAULT(11);
        END

        IF COL_LENGTH('AccountingDocumentAttachments', 'FileData') IS NULL
        BEGIN
            ALTER TABLE [AccountingDocumentAttachments] ADD [FileData] varbinary(max) NULL;
        END
        """);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
