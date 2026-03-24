1. Local Database Configuration (macOS Docker)
   The project uses a local SQL Server instance running via Docker.

Connection String:
The appsettings.Development.json file MUST contain the following connection string exactly as written. It includes the TrustServerCertificate=True flag required for local Docker SQL Server environments.

JSON
{
"ConnectionStrings": {
"DefaultConnection": "Server=localhost,1433;Database=CucinaCaninaDb;User Id=sa;Password=Str0ng!Passw0rd123;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
} 2. SQL Server Table Schema
The database uses Entity Framework Core 8 Code-First migration. Below is the explicit schema definition mapping to the C# Entities.

Table: Ingredients

Stores all types of ingredients (Raw, Cooked, Vitamins).

Id (UNIQUEIDENTIFIER, PK) - Primary Key (Guid in C#).

Name (NVARCHAR(100), NOT NULL) - Name of the ingredient.

Category (INT, NOT NULL) - Maps to IngredientCategory enum (0 = RawFood, 1 = CookedFood, 2 = Vitamins).

BasePricePerKg (DECIMAL(18,4), NOT NULL) - Base price.

RecommendedPriceMultiplier (DECIMAL(18,4), NOT NULL) - Multiplier for calculating recommended price (default 2.0 or 1.2).

ReferenceIngredientId (UNIQUEIDENTIFIER, NULL) - Foreign Key self-referencing Ingredients(Id). Populated ONLY when Category == 1 (CookedFood). On Delete rule should be Restrict or SetNull to avoid multiple cascade paths.

CookedMultiplier (DECIMAL(18,4), NULL) - Multiplier for weight reduction when cooking.

Table: Recipes

Stores the recipes created by the administrator.

Id (UNIQUEIDENTIFIER, PK) - Primary Key (Guid in C#).

Name (NVARCHAR(150), NOT NULL) - Name of the recipe.

IdealPrice (DECIMAL(18,4), NOT NULL) - The target sale price per portion set by the admin.

Table: RecipeIngredients (Join Table)

Maps the many-to-many relationship between Recipes and Ingredients, storing the specific quantity used.

RecipeId (UNIQUEIDENTIFIER, PK, FK) - Foreign Key referencing Recipes(Id).

IngredientId (UNIQUEIDENTIFIER, PK, FK) - Foreign Key referencing Ingredients(Id).

QuantityGrams (DECIMAL(18,4), NOT NULL) - Amount of the ingredient used in the recipe for a single portion.

Note: The Primary Key for this table is a composite key: (RecipeId, IngredientId).

3. EF Core Fluent API Rules (AppDbContext.cs)
   When configuring the AppDbContext, the AI must enforce the following rules in OnModelCreating:

Decimal Precision: All decimal properties MUST be configured with .HasPrecision(18, 4) to avoid SQL Server truncation warnings and ensure accurate financial calculations.

Self-Referencing Key Configuration:

C#
modelBuilder.Entity<Ingredient>()
.HasOne(i => i.ReferenceIngredient)
.WithMany()
.HasForeignKey(i => i.ReferenceIngredientId)
.OnDelete(DeleteBehavior.Restrict); // Critical to prevent cascade delete errors in SQL Server
Composite Key:

C#
modelBuilder.Entity<RecipeIngredient>()
.HasKey(ri => new { ri.RecipeId, ri.IngredientId });
