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
