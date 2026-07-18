using InteriorCalculator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InteriorCalculator.Api.Data;

public static class PriceItemSeeder
{
    private static readonly HashSet<string> ObsoleteSystemCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "HEADBOARD",
        "BEDSIDE_CLADDING",
        "DUVET"
    };

    public static async Task<int> SeedAsync(AppDbContext context)
    {
        var existingItems = await context.PriceItems
            .ToListAsync();

        var existingByCode = existingItems.ToDictionary(p => p.Code, StringComparer.OrdinalIgnoreCase);
        var changedCount = 0;

        foreach (var catalogueItem in GetPriceItems())
        {
            if (!existingByCode.TryGetValue(catalogueItem.Code, out var existingItem))
            {
                context.PriceItems.Add(catalogueItem);
                changedCount++;
                continue;
            }

            if (!NeedsUpdate(existingItem, catalogueItem))
                continue;

            existingItem.Name = catalogueItem.Name;
            existingItem.Category = catalogueItem.Category;
            existingItem.Unit = catalogueItem.Unit;
            existingItem.VariableType = catalogueItem.VariableType;

            if (ShouldForceInactive(catalogueItem.Code))
                existingItem.IsActive = false;

            existingItem.UpdatedAt = DateTime.UtcNow;
            changedCount++;
        }

        if (changedCount > 0)
            await context.SaveChangesAsync();

        return changedCount;
    }

    private static List<PriceItem> GetPriceItems()
    {
        // TEMPORARY TEST RATES.
        // Replace rates and units with client-approved production values before deployment.
        // Seed rates are initial test defaults only.
        // Existing database rates are preserved because rates will be managed by admins.
        return
        [
            Item("DESIGN_MODERN", "Modern Design", PriceCategory.Design, 25000m, "fixed", VariableType.Fixed),
            Item("DESIGN_NEO_CLASSIC", "Neoclassic Design", PriceCategory.Design, 30000m, "fixed", VariableType.Fixed),
            Item("DESIGN_CLASSIC", "Classic Design", PriceCategory.Design, 28000m, "fixed", VariableType.Fixed),

            Item("CEILING_LEVEL_1", "Gypsum Ceiling Level 1", PriceCategory.Ceiling, 180m, "sqft", VariableType.Area),
            Item("CEILING_LEVEL_2", "Gypsum Ceiling Level 2", PriceCategory.Ceiling, 240m, "sqft", VariableType.Area),
            Item("CORNISH_5CM", "Cornish 5cm", PriceCategory.Ceiling, 120m, "rft", VariableType.Length),
            Item("CORNISH_10CM", "Cornish 10cm", PriceCategory.Ceiling, 180m, "rft", VariableType.Length),
            Item("LIGHT_TRACK", "Track Light", PriceCategory.Ceiling, 900m, "piece", VariableType.Quantity),
            Item("LIGHT_SPOT", "Spot Light", PriceCategory.Ceiling, 500m, "piece", VariableType.Quantity),
            Item("LIGHT_HIDDEN", "Hidden Light", PriceCategory.Ceiling, 650m, "piece", VariableType.Quantity),
            Item("LIGHT_STRIP", "Light Strip", PriceCategory.Ceiling, 180m, "piece", VariableType.Quantity),
            Item("CHANDELIER", "Chandelier", PriceCategory.Ceiling, 8500m, "piece", VariableType.Quantity),
            Item("CURTAIN_BOX", "Curtain/Cornish Box", PriceCategory.Ceiling, 300m, "rft", VariableType.Length),
            Item("CEILING_PAINTING", "Ceiling Painting", PriceCategory.Ceiling, 50m, "sqft", VariableType.Area),

            Item("CURTAIN_CHOICE_1", "Curtain Choice 1", PriceCategory.Walls, 450m, "rft", VariableType.Length),
            Item("CURTAIN_CHOICE_2", "Curtain Choice 2", PriceCategory.Walls, 650m, "rft", VariableType.Length),
            Item("WALL_PAINT_CHOICE_1", "Wall Paint Choice 1", PriceCategory.Walls, 45m, "sqft", VariableType.Area),
            Item("WALL_PAINT_CHOICE_2", "Wall Paint Choice 2", PriceCategory.Walls, 70m, "sqft", VariableType.Area),
            Item("WALLPAPER_CHOICE_1", "Wallpaper Choice 1", PriceCategory.Walls, 90m, "sqft", VariableType.Area),
            Item("WALLPAPER_CHOICE_2", "Wallpaper Choice 2", PriceCategory.Walls, 140m, "sqft", VariableType.Area),
            Item("WALL_MOULDING", "Wall Moulding", PriceCategory.Walls, 160m, "rft", VariableType.Length),
            Item("DOOR_CHANGED", "Door Changed", PriceCategory.Walls, 15000m, "piece", VariableType.Quantity),
            Item("WINDOW_CHANGED", "Window Changed", PriceCategory.Walls, 12000m, "piece", VariableType.Quantity),
            Item("WALL_CLADDING", "Wall Cladding", PriceCategory.Walls, 350m, "sqft", VariableType.Area),

            Item("FLOOR_PORCELAIN_120", "Porcelain 120 Flooring", PriceCategory.Flooring, 220m, "sqft", VariableType.Area),
            Item("FLOOR_PORCELAIN_60", "Porcelain 60 Flooring", PriceCategory.Flooring, 160m, "sqft", VariableType.Area),
            Item("FLOOR_MARBLE_120", "Marble 120 Flooring", PriceCategory.Flooring, 450m, "sqft", VariableType.Area),
            Item("FLOOR_MARBLE_60", "Marble 60 Flooring", PriceCategory.Flooring, 320m, "sqft", VariableType.Area),
            Item("FLOOR_GRANITE_120", "Granite 120 Flooring", PriceCategory.Flooring, 360m, "sqft", VariableType.Area),
            Item("FLOOR_GRANITE_60", "Granite 60 Flooring", PriceCategory.Flooring, 260m, "sqft", VariableType.Area),
            Item("SKIRTING_10", "Skirting 10cm", PriceCategory.Flooring, 90m, "rft", VariableType.Length),
            Item("SKIRTING_15", "Skirting 15cm", PriceCategory.Flooring, 130m, "rft", VariableType.Length),
            Item("PARQUET", "Parquet", PriceCategory.Flooring, 240m, "sqft", VariableType.Area),
            Item("GLASS_WORK", "Glass Work", PriceCategory.Flooring, 500m, "sqft", VariableType.Area),

            Item("BED_KING", "King Bed", PriceCategory.Furnishing, 55000m, "fixed", VariableType.Fixed),
            Item("BED_QUEEN", "Queen Bed", PriceCategory.Furnishing, 45000m, "fixed", VariableType.Fixed),
            Item("HEADBOARD", "Headboard", PriceCategory.Furnishing, 9000m, "piece", VariableType.Quantity, false),
            Item("BEDSIDE_CLADDING", "Bedside Cladding", PriceCategory.Furnishing, 280m, "sqft", VariableType.Area, false),
            Item("DUVET", "Duvet", PriceCategory.Furnishing, 6500m, "piece", VariableType.Quantity, false),
            Item("HEADBOARD_CLADDING", "Headboard/Cladding", PriceCategory.Furnishing, 300m, "sqft", VariableType.Area),
            Item("SIDE_TABLE", "Side Table", PriceCategory.Furnishing, 8000m, "piece", VariableType.Quantity),
            Item("SIDE_LAMP", "Side Lamp", PriceCategory.Furnishing, 3000m, "piece", VariableType.Quantity),
            Item("TV_UNIT", "TV Unit", PriceCategory.Furnishing, 28000m, "fixed", VariableType.Fixed),
            Item("CHAIR", "Chair", PriceCategory.Furnishing, 7000m, "piece", VariableType.Quantity),
            Item("STOOL", "Stool", PriceCategory.Furnishing, 3500m, "piece", VariableType.Quantity),
            Item("DRESSING_TABLE", "Dressing Table", PriceCategory.Furnishing, 22000m, "fixed", VariableType.Fixed),
            Item("CARPET", "Carpet", PriceCategory.Furnishing, 80m, "sqft", VariableType.Area),
            Item("BENCH", "Bench", PriceCategory.Furnishing, 10000m, "fixed", VariableType.Fixed),
            Item("AC_SPLIT", "Split AC", PriceCategory.Furnishing, 42000m, "piece", VariableType.Quantity),
            Item("AC_CASSETTE", "Cassette AC", PriceCategory.Furnishing, 75000m, "piece", VariableType.Quantity),

            Item("ADDITIONAL_MANUAL", "Additional Manual Item", PriceCategory.Additional, 0m, "manual", VariableType.Manual)
        ];
    }

    private static bool NeedsUpdate(PriceItem existingItem, PriceItem catalogueItem)
    {
        return existingItem.Name != catalogueItem.Name
            || existingItem.Category != catalogueItem.Category
            || existingItem.Unit != catalogueItem.Unit
            || existingItem.VariableType != catalogueItem.VariableType
            || ShouldForceInactive(catalogueItem.Code) && existingItem.IsActive;
    }

    private static bool ShouldForceInactive(string code)
    {
        return ObsoleteSystemCodes.Contains(code);
    }

    private static PriceItem Item(
        string code,
        string name,
        PriceCategory category,
        decimal rate,
        string unit,
        VariableType variableType,
        bool isActive = true)
    {
        var now = DateTime.UtcNow;

        return new PriceItem
        {
            Code = code,
            Name = name,
            Category = category,
            Rate = rate,
            Unit = unit,
            VariableType = variableType,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
