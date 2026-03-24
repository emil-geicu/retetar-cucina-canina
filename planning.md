1. Descrierea Proiectului
   Cucina Canina este o aplicație internă pentru gestionarea rețetelor de mâncare gătită pentru animale de companie. Aplicația gestionează ingrediente (crude, gătite, vitamine), rețete și calculează în timp real costurile și marjele de profit în funcție de gramajele folosite și prețurile setate de administrator.

IMPORTANT - REGULĂ DE LIMBĂ: \* Codul sursă (C#): Numele claselor, variabilelor, metodelor și tabelelor din baza de date vor fi în limba Engleză (ex: Ingredient, Recipe).

Interfața Utilizator (UI): Toate textele vizibile pe ecran (etichete, butoane, mesaje de eroare, capete de tabel, meniuri) vor fi scrise exclusiv în limba Română (ex: "Adaugă Rețetă", "Preț Recomandat", "Pui Gătit").

2. Tech Stack
   Framework: Blazor Server pe .NET 8 (LTS).

Database: SQL Server (LocalDB pentru dezvoltare).

ORM: Entity Framework Core 8.

Styling: CSS Custom / Bootstrap (sau MudBlazor) pentru un UI minimalist și interactiv.

3. Arhitectura Bazei de Date (Entity Framework Core)
   3.1. Enums (Categorii Ingrediente)

C#
public enum IngredientCategory {
RawFood, // Afișat în UI ca "Aliment Crud" (ex: pui, vită, porc)
CookedFood, // Afișat în UI ca "Aliment Gătit"
Vitamins // Afișat în UI ca "Vitamine"
}
3.2. Entități

Ingredient (Ingredient)

Id (Guid / int)

Name (string)

Category (IngredientCategory)

BasePricePerKg (decimal)

RecommendedPriceMultiplier (decimal) - Default: 2.0 pentru Raw/Cooked, 1.2 pentru Vitamins. Modificabil din UI.

ReferenceIngredientId (Guid? / int?) - Foreign Key. Folosit DOAR pentru CookedFood (indică spre alimentul crud).

ReferenceIngredient (Ingredient)

CookedMultiplier (decimal?) - Ex: 2.0 (1 kg pui gătit = 2 kg pui crud).

Recipe (Rețetă)

Id (Guid / int)

Name (string)

IdealPrice (decimal) - Prețul ideal per o singură porție/rețetă.

RecipeIngredients (ICollection<RecipeIngredient>)

RecipeIngredient (Tabel de legătură)

RecipeId (Guid / int)

IngredientId (Guid / int)

QuantityGrams (decimal) - Cantitatea în grame pentru o porție.

Ingredient

Recipe

4. Logica de Business și Formule de Calcul
   Cost Real per Kg (Ingredient):

RawFood / Vitamins: Cost = BasePricePerKg.

CookedFood: Cost = ReferenceIngredient.BasePricePerKg \* CookedMultiplier.

Cost Real per Gram: Cost Real per Kg / 1000.

Cost Recomandat per Kg: Cost Real per Kg \* RecommendedPriceMultiplier.

Preț Recomandat per Ingredient (1 porție): (Cost Recomandat per Kg / 1000) \* QuantityGrams.

Preț Recomandat Total (1 porție): Suma(Preț Recomandat per Ingredient).

Diferența de Preț (1 porție): IdealPrice - Preț Recomandat Total.

Status Culoare (pentru Diferență):

> 0 -> Verde (Profit pozitiv)

== 0 -> Galben (Egalitate)

< 0 -> Roșu (Pierdere sau sub prețul recomandat)

5. UI/UX & Design "Cute Pet Kitchen"
   Tema: Minimalistă, prietenoasă, bucătărie pentru animale.

Paleta de culori: Culori calde.

Fundal: Alb crem / Bej deschis (#FAF6F0).

Accente: Portocaliu pastel, Maro cald, Galben unt.

Text: Maro închis/Gri antracit.

Tipografie: Fonturi rotunjite, prietenoase (ex: Quicksand, Nunito, Poppins).

Componente: Carduri cu colțuri rotunjite, umbre fine.

6. Ecranele Aplicației (Texte în Română)
1. Panou Rețete (/recipes)

Listă/Grid cu rețetele existente.

Bară de căutare: "Caută după nume rețetă sau ingredient...".

Butoane pe fiecare rețetă: [Detalii], [Editează], [Șterge].

2. Detalii Rețetă (/recipes/details/{id}) - Mod Interactiv

Antet: Numele rețetei.

Multiplicator Porții: Câmp de input numeric, denumit "Număr de porții" (valoarea implicită = 1). La modificarea acestei valori, toate numerele afișate pe pagină se multiplică dinamic (doar vizual, fără a salva în baza de date).

Sumar Financiar (sus) - Se înmulțesc cu "Număr de porții":

Preț Ideal Total: IdealPrice \* Număr Porții

Preț Recomandat Total: TotalRecommendedPrice \* Număr Porții

Diferență Totală: (IdealPrice - TotalRecommendedPrice) \* Număr Porții (cu status de culoare).

Tabel Ingrediente - Se înmulțesc cu "Număr de porții":

Nume Ingredient

Cantitate Necesară: QuantityGrams \* Număr Porții

Preț/Gramaj (pentru cantitatea calculată)

Preț Recomandat (pentru cantitatea calculată).

3. Formular Creare/Editare Rețetă (/recipes/create, /recipes/edit/{id})

Câmp text: "Nume Rețetă".

Câmp numeric: "Preț Ideal (per porție)".

Secțiune interactivă: Adaugă/șterge ingrediente.

Câmp numeric pentru "Cantitate (grame)" cu actualizare în timp real a costurilor pe linie.

Subsol/Antet fix care afișează calculele în timp real (Total Recomandat, Diferența, Culoarea).

4. Formular Creare/Editare Ingredient (/ingredients/create, /ingredients/edit/{id})

Câmp text: "Nume Ingredient".

Dropdown Categorie: "Aliment Crud", "Aliment Gătit", "Vitamine".

Funcționalități dinamice în funcție de categorie (afișare preț per kg sau referință către ingredient crud + multiplicator gătire).

Câmp numeric: "Multiplicator Preț Recomandat" (precompletat 2.0 sau 1.2).

5. Panou Ingrediente (/ingredients)

Tabel cu toate ingredientele. Bară de căutare.

Coloane: Nume, Categorie, Preț/Kg, Multiplicator Preț, [Acțiuni: Editează, Șterge].
