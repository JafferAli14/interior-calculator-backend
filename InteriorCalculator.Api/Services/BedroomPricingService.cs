using InteriorCalculator.Api.Data;
using InteriorCalculator.Api.DTOs;
using InteriorCalculator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InteriorCalculator.Api.Services;

public sealed class BedroomPricingValidationException : Exception
{
    public BedroomPricingValidationException(List<string> errors)
        : base("Bedroom planner request validation failed.")
    {
        Errors = errors;
    }

    public List<string> Errors { get; }
}

public sealed class BedroomPricingService
{
    private const string SupportedSchemaVersion = "bedroom-planner.v2";
    private const string TemporaryCurrency = "QAR";
    private readonly AppDbContext _context;

    public BedroomPricingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BedroomPreviewResponseDto> PreviewAsync(BedroomPlannerRequestDto request)
    {
        var errors = new List<string>();
        ValidateTopLevel(request, errors);

        var requestedCodes = CollectRequestedCodes(request)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var priceItems = await _context.PriceItems
            .AsNoTracking()
            .Where(item => requestedCodes.Contains(item.Code))
            .ToListAsync();

        var priceItemsByCode = priceItems.ToDictionary(item => item.Code, StringComparer.OrdinalIgnoreCase);
        var lines = new List<ProjectPriceLineDto>();
        var sortOrder = 10;

        AddDesignLine(request.Design, priceItemsByCode, lines, errors, ref sortOrder);
        AddCeilingLines(request, priceItemsByCode, lines, errors, ref sortOrder);
        AddWallLines(request, priceItemsByCode, lines, errors, ref sortOrder);
        AddFlooringLines(request, priceItemsByCode, lines, errors, ref sortOrder);
        AddFurnishingLines(request, priceItemsByCode, lines, errors, ref sortOrder);
        AddAdditionalRequirementLines(request.AdditionalRequirements, lines, errors);

        if (errors.Count > 0)
            throw new BedroomPricingValidationException(errors);

        var subtotals = lines
            .GroupBy(line => line.Category)
            .Select(group => new CategorySubtotalDto
            {
                Category = group.Key,
                Amount = RoundMoney(group.Sum(line => line.FinalAmount))
            })
            .OrderBy(subtotal => CategorySortOrder(subtotal.Category))
            .ToList();

        return new BedroomPreviewResponseDto
        {
            SchemaVersion = SupportedSchemaVersion,
            Currency = TemporaryCurrency,
            PriceLines = lines.OrderBy(line => line.SortOrder).ToList(),
            CategorySubtotals = subtotals,
            GrandTotal = RoundMoney(subtotals.Sum(subtotal => subtotal.Amount)),
            Warnings = []
        };
    }

    private static void ValidateTopLevel(BedroomPlannerRequestDto request, List<string> errors)
    {
        if (request.SchemaVersion != SupportedSchemaVersion)
            errors.Add($"SchemaVersion must be '{SupportedSchemaVersion}'.");

        if (string.IsNullOrWhiteSpace(request.ProjectName))
            errors.Add("ProjectName is required.");
        else if (request.ProjectName.Length > 150)
            errors.Add("ProjectName must be 150 characters or fewer.");

        if (request.ClientName?.Length > 100)
            errors.Add("ClientName must be 100 characters or fewer.");

        if (request.ClientMobile?.Length > 20)
            errors.Add("ClientMobile must be 20 characters or fewer.");

        if (request.Currency != TemporaryCurrency)
            errors.Add($"Currency must be '{TemporaryCurrency}' for this phase.");

        if (request.Measurements.RoomLength is <= 0)
            errors.Add("Measurements.RoomLength must be greater than zero when provided.");

        if (request.Measurements.RoomWidth is <= 0)
            errors.Add("Measurements.RoomWidth must be greater than zero when provided.");
    }

    private static IEnumerable<string?> CollectRequestedCodes(BedroomPlannerRequestDto request)
    {
        yield return request.Design.PriceItemCode;
        yield return request.Ceiling.GypsumCeiling?.PriceItemCode;
        yield return request.Ceiling.Cornish?.PriceItemCode;
        yield return request.Ceiling.Chandelier?.PriceItemCode;
        yield return request.Ceiling.CurtainBox?.PriceItemCode;
        yield return request.Ceiling.CeilingPainting?.PriceItemCode;
        yield return request.Walls.Curtain?.PriceItemCode;
        yield return request.Walls.Moulding?.PriceItemCode;
        yield return request.Walls.WallPainting?.PriceItemCode;
        yield return request.Walls.Wallpaper?.PriceItemCode;
        yield return request.Walls.Doors?.PriceItemCode;
        yield return request.Walls.Windows?.PriceItemCode;
        yield return request.Walls.Cladding?.PriceItemCode;
        yield return request.Flooring.Tiles?.PriceItemCode;
        yield return request.Flooring.Skirting?.PriceItemCode;
        yield return request.Flooring.Parquet?.PriceItemCode;
        yield return request.Flooring.Glasswork?.PriceItemCode;
        yield return request.Furnishing.Bed?.PriceItemCode;
        yield return request.Furnishing.HeadboardCladding?.PriceItemCode;
        yield return request.Furnishing.SideTable?.PriceItemCode;
        yield return request.Furnishing.SideLamps?.PriceItemCode;
        yield return request.Furnishing.TvUnit?.PriceItemCode;
        yield return request.Furnishing.Chairs?.PriceItemCode;
        yield return request.Furnishing.Stools?.PriceItemCode;
        yield return request.Furnishing.DressingTable?.PriceItemCode;
        yield return request.Furnishing.Carpet?.PriceItemCode;
        yield return request.Furnishing.Bench?.PriceItemCode;
        yield return request.Furnishing.Ac?.PriceItemCode;

        foreach (var light in request.Ceiling.CeilingLights)
            yield return light.PriceItemCode;
    }

    private static void AddDesignLine(
        BedroomDesignDto design,
        IReadOnlyDictionary<string, PriceItem> priceItems,
        List<ProjectPriceLineDto> lines,
        List<string> errors,
        ref int sortOrder)
    {
        var item = ResolvePriceItem(design.PriceItemCode, "Design", PriceCategory.Design, VariableType.Fixed, priceItems, errors);
        if (item == null)
            return;

        AddCalculatedLine(lines, ReportCategoryDto.Design, item, null, null, null, item.Rate, "Fixed price", ref sortOrder);
    }

    private static void AddCeilingLines(
        BedroomPlannerRequestDto request,
        IReadOnlyDictionary<string, PriceItem> priceItems,
        List<ProjectPriceLineDto> lines,
        List<string> errors,
        ref int sortOrder)
    {
        AddCanonicalAreaLine(request.Ceiling.GypsumCeiling, request.Measurements.CeilingArea, "Ceiling.GypsumCeiling", ReportCategoryDto.Ceiling, PriceCategory.Ceiling, priceItems, lines, errors, ref sortOrder);
        AddLengthLine(request.Ceiling.Cornish, "Ceiling.Cornish", ReportCategoryDto.Ceiling, PriceCategory.Ceiling, priceItems, lines, errors, ref sortOrder);

        var enabledLights = request.Ceiling.CeilingLights
            .Where(light => light.Enabled)
            .ToList();
        var duplicateLightCodes = enabledLights
            .Where(light => !string.IsNullOrWhiteSpace(light.PriceItemCode))
            .GroupBy(light => light.PriceItemCode!, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key);

        foreach (var duplicateCode in duplicateLightCodes)
            errors.Add($"Ceiling.CeilingLights contains duplicate PriceItemCode '{duplicateCode}'.");

        foreach (var light in enabledLights)
            AddQuantityLine(light, "Ceiling.CeilingLights", ReportCategoryDto.Ceiling, PriceCategory.Ceiling, priceItems, lines, errors, ref sortOrder);

        AddCustomQuantityLine(request.Ceiling.Chandelier, "Ceiling.Chandelier", ReportCategoryDto.Ceiling, PriceCategory.Ceiling, VariableType.Quantity, priceItems, lines, errors, ref sortOrder);
        AddLengthLine(request.Ceiling.CurtainBox, "Ceiling.CurtainBox", ReportCategoryDto.Ceiling, PriceCategory.Ceiling, priceItems, lines, errors, ref sortOrder);
        AddPaintCanonicalAreaLine(request.Ceiling.CeilingPainting, request.Measurements.CeilingArea, "Ceiling.CeilingPainting", ReportCategoryDto.Ceiling, PriceCategory.Ceiling, priceItems, lines, errors, ref sortOrder);
    }

    private static void AddWallLines(
        BedroomPlannerRequestDto request,
        IReadOnlyDictionary<string, PriceItem> priceItems,
        List<ProjectPriceLineDto> lines,
        List<string> errors,
        ref int sortOrder)
    {
        AddLengthLine(request.Walls.Curtain, "Walls.Curtain", ReportCategoryDto.Walls, PriceCategory.Walls, priceItems, lines, errors, ref sortOrder);
        AddLengthLine(request.Walls.Moulding, "Walls.Moulding", ReportCategoryDto.Walls, PriceCategory.Walls, priceItems, lines, errors, ref sortOrder);
        AddCustomPaintAreaLine(request.Walls.WallPainting, request.Measurements.WallArea, "Walls.WallPainting", ReportCategoryDto.Walls, PriceCategory.Walls, priceItems, lines, errors, ref sortOrder);
        AddCanonicalAreaLine(request.Walls.Wallpaper, request.Measurements.WallArea, "Walls.Wallpaper", ReportCategoryDto.Walls, PriceCategory.Walls, priceItems, lines, errors, ref sortOrder);
        AddQuantityLine(request.Walls.Doors, "Walls.Doors", ReportCategoryDto.Walls, PriceCategory.Walls, priceItems, lines, errors, ref sortOrder);
        AddQuantityLine(request.Walls.Windows, "Walls.Windows", ReportCategoryDto.Walls, PriceCategory.Walls, priceItems, lines, errors, ref sortOrder);
        AddAreaLine(request.Walls.Cladding, "Walls.Cladding", ReportCategoryDto.Walls, PriceCategory.Walls, priceItems, lines, errors, ref sortOrder);
    }

    private static void AddFlooringLines(
        BedroomPlannerRequestDto request,
        IReadOnlyDictionary<string, PriceItem> priceItems,
        List<ProjectPriceLineDto> lines,
        List<string> errors,
        ref int sortOrder)
    {
        AddTileLine(request.Flooring.Tiles, request.Measurements.FlooringArea, priceItems, lines, errors, ref sortOrder);
        AddLengthLine(request.Flooring.Skirting, "Flooring.Skirting", ReportCategoryDto.Flooring, PriceCategory.Flooring, priceItems, lines, errors, ref sortOrder);
        AddAreaLine(request.Flooring.Parquet, "Flooring.Parquet", ReportCategoryDto.Flooring, PriceCategory.Flooring, priceItems, lines, errors, ref sortOrder);
        AddAreaLine(request.Flooring.Glasswork, "Flooring.Glasswork", ReportCategoryDto.Flooring, PriceCategory.Flooring, priceItems, lines, errors, ref sortOrder);
    }

    private static void AddFurnishingLines(
        BedroomPlannerRequestDto request,
        IReadOnlyDictionary<string, PriceItem> priceItems,
        List<ProjectPriceLineDto> lines,
        List<string> errors,
        ref int sortOrder)
    {
        AddFixedLine(request.Furnishing.Bed, "Furnishing.Bed", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddCustomAreaLine(request.Furnishing.HeadboardCladding, "Furnishing.HeadboardCladding", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddQuantityLine(request.Furnishing.SideTable, "Furnishing.SideTable", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddQuantityLine(request.Furnishing.SideLamps, "Furnishing.SideLamps", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddCustomFixedLine(request.Furnishing.TvUnit, "Furnishing.TvUnit", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddQuantityLine(request.Furnishing.Chairs, "Furnishing.Chairs", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddQuantityLine(request.Furnishing.Stools, "Furnishing.Stools", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddFixedLine(request.Furnishing.DressingTable, "Furnishing.DressingTable", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddAreaLine(request.Furnishing.Carpet, "Furnishing.Carpet", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddFixedLine(request.Furnishing.Bench, "Furnishing.Bench", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, priceItems, lines, errors, ref sortOrder);
        AddCustomQuantityLine(request.Furnishing.Ac, "Furnishing.Ac", ReportCategoryDto.Furnishing, PriceCategory.Furnishing, VariableType.Quantity, priceItems, lines, errors, ref sortOrder);
    }

    private static void AddAdditionalRequirementLines(
        List<AdditionalRequirementDto> additionalRequirements,
        List<ProjectPriceLineDto> lines,
        List<string> errors)
    {
        foreach (var requirement in additionalRequirements)
        {
            if (string.IsNullOrWhiteSpace(requirement.ItemName))
                errors.Add("AdditionalRequirements.ItemName is required.");

            if (requirement.CustomPrice <= 0)
                errors.Add($"AdditionalRequirements '{requirement.ItemName}' CustomPrice must be greater than zero.");

            if (string.IsNullOrWhiteSpace(requirement.ItemName) || requirement.CustomPrice <= 0)
                continue;

            var amount = RoundMoney(requirement.CustomPrice);
            lines.Add(new ProjectPriceLineDto
            {
                Category = requirement.Category,
                ItemCode = null,
                ItemName = requirement.ItemName.Trim(),
                PricingMode = PricingModeDto.Custom,
                Selection = requirement.Description,
                Unit = "manual",
                RateUsed = null,
                CustomPrice = amount,
                CalculationText = "Manually entered price",
                FinalAmount = amount,
                SortOrder = 1000 + requirement.SortOrder
            });
        }
    }

    private static void AddFixedLine(EnabledCodeItemDto? request, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, VariableType.Fixed, priceItems, errors);
        if (item == null)
            return;

        AddCalculatedLine(lines, reportCategory, item, null, null, null, item.Rate, "Fixed price", ref sortOrder);
    }

    private static void AddQuantityLine(QuantityCodeItemDto? request, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, VariableType.Quantity, priceItems, errors);
        if (item == null)
            return;

        if (!HasPositiveValue(request.Quantity))
        {
            errors.Add($"{path}.Quantity must be greater than zero.");
            return;
        }

        var amount = item.Rate * request.Quantity!.Value;
        AddCalculatedLine(lines, reportCategory, item, request.Quantity, null, null, amount, $"{FormatMoney(item.Rate)} x {request.Quantity} {item.Unit}", ref sortOrder);
    }

    private static void AddAreaLine(AreaItemDto? request, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, VariableType.Area, priceItems, errors);
        if (item == null)
            return;

        if (!HasPositiveValue(request.Area))
        {
            errors.Add($"{path}.Area must be greater than zero.");
            return;
        }

        var amount = item.Rate * request.Area!.Value;
        AddCalculatedLine(lines, reportCategory, item, null, request.Area, null, amount, $"{FormatMoney(item.Rate)} x {FormatMeasure(request.Area)} {item.Unit}", ref sortOrder);
    }

    private static void AddCanonicalAreaLine(EnabledCodeItemDto? request, decimal? area, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, VariableType.Area, priceItems, errors);
        if (item == null)
            return;

        if (!HasPositiveValue(area))
        {
            errors.Add($"{path} requires a canonical area greater than zero.");
            return;
        }

        var amount = item.Rate * area!.Value;
        AddCalculatedLine(lines, reportCategory, item, null, area, null, amount, $"{FormatMoney(item.Rate)} x {FormatMeasure(area)} {item.Unit}", ref sortOrder);
    }

    private static void AddPaintCanonicalAreaLine(PaintCodeItemDto? request, decimal? area, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, VariableType.Area, priceItems, errors);
        if (item == null)
            return;

        if (!HasPositiveValue(area))
        {
            errors.Add($"{path} requires a canonical area greater than zero.");
            return;
        }

        var amount = item.Rate * area!.Value;
        AddCalculatedLine(lines, reportCategory, item, null, area, null, amount, $"{FormatMoney(item.Rate)} x {FormatMeasure(area)} {item.Unit}", ref sortOrder, request.PaintColour);
    }

    private static void AddLengthLine(LengthItemDto? request, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, VariableType.Length, priceItems, errors);
        if (item == null)
            return;

        if (!HasPositiveValue(request.Length))
        {
            errors.Add($"{path}.Length must be greater than zero.");
            return;
        }

        var amount = item.Rate * request.Length!.Value;
        AddCalculatedLine(lines, reportCategory, item, null, null, request.Length, amount, $"{FormatMoney(item.Rate)} x {FormatMeasure(request.Length)} {item.Unit}", ref sortOrder);
    }

    private static void AddTileLine(TileItemDto? request, decimal? flooringArea, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, "Flooring.Tiles", PriceCategory.Flooring, VariableType.Area, priceItems, errors);
        if (item == null)
            return;

        if (!HasPositiveValue(flooringArea))
        {
            errors.Add("Flooring.Tiles requires Measurements.FlooringArea greater than zero.");
            return;
        }

        if (string.IsNullOrWhiteSpace(request.Material))
            errors.Add("Flooring.Tiles.Material is required when tiles are enabled.");

        if (string.IsNullOrWhiteSpace(request.TileSize))
            errors.Add("Flooring.Tiles.TileSize is required when tiles are enabled.");

        var amount = item.Rate * flooringArea!.Value;
        AddCalculatedLine(lines, ReportCategoryDto.Flooring, item, null, flooringArea, null, amount, $"{FormatMoney(item.Rate)} x {FormatMeasure(flooringArea)} {item.Unit}", ref sortOrder, $"{request.Material} {request.TileSize}".Trim());
    }

    private static void AddCustomQuantityLine(CustomQuantityItemDto? request, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, VariableType expectedVariableType, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, expectedVariableType, priceItems, errors);
        if (item == null || !ValidatePricingMode(request.PricingMode, path, errors))
            return;

        if (request.PricingMode == PricingModeDto.Custom)
        {
            AddCustomLine(request.CustomPrice, path, reportCategory, item, lines, errors, ref sortOrder);
            return;
        }

        if (!HasPositiveValue(request.Quantity))
        {
            errors.Add($"{path}.Quantity must be greater than zero in calculated mode.");
            return;
        }

        var amount = item.Rate * request.Quantity!.Value;
        AddCalculatedLine(lines, reportCategory, item, request.Quantity, null, null, amount, $"{FormatMoney(item.Rate)} x {request.Quantity} {item.Unit}", ref sortOrder);
    }

    private static void AddCustomAreaLine(CustomAreaItemDto? request, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, VariableType.Area, priceItems, errors);
        if (item == null || !ValidatePricingMode(request.PricingMode, path, errors))
            return;

        if (request.PricingMode == PricingModeDto.Custom)
        {
            AddCustomLine(request.CustomPrice, path, reportCategory, item, lines, errors, ref sortOrder);
            return;
        }

        if (!HasPositiveValue(request.Area))
        {
            errors.Add($"{path}.Area must be greater than zero in calculated mode.");
            return;
        }

        var amount = item.Rate * request.Area!.Value;
        AddCalculatedLine(lines, reportCategory, item, null, request.Area, null, amount, $"{FormatMoney(item.Rate)} x {FormatMeasure(request.Area)} {item.Unit}", ref sortOrder);
    }

    private static void AddCustomPaintAreaLine(CustomPaintItemDto? request, decimal? wallArea, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, VariableType.Area, priceItems, errors);
        if (item == null || !ValidatePricingMode(request.PricingMode, path, errors))
            return;

        if (request.PricingMode == PricingModeDto.Custom)
        {
            AddCustomLine(request.CustomPrice, path, reportCategory, item, lines, errors, ref sortOrder, request.PaintColour);
            return;
        }

        if (!HasPositiveValue(wallArea))
        {
            errors.Add($"{path} requires Measurements.WallArea greater than zero in calculated mode.");
            return;
        }

        var amount = item.Rate * wallArea!.Value;
        AddCalculatedLine(lines, reportCategory, item, null, wallArea, null, amount, $"{FormatMoney(item.Rate)} x {FormatMeasure(wallArea)} {item.Unit}", ref sortOrder, request.PaintColour);
    }

    private static void AddCustomFixedLine(CustomFixedItemDto? request, string path, ReportCategoryDto reportCategory, PriceCategory expectedCategory, IReadOnlyDictionary<string, PriceItem> priceItems, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder)
    {
        if (request is not { Enabled: true })
            return;

        var item = ResolvePriceItem(request.PriceItemCode, path, expectedCategory, VariableType.Fixed, priceItems, errors);
        if (item == null || !ValidatePricingMode(request.PricingMode, path, errors))
            return;

        if (request.PricingMode == PricingModeDto.Custom)
        {
            AddCustomLine(request.CustomPrice, path, reportCategory, item, lines, errors, ref sortOrder);
            return;
        }

        AddCalculatedLine(lines, reportCategory, item, null, null, null, item.Rate, "Fixed price", ref sortOrder);
    }

    private static PriceItem? ResolvePriceItem(string? code, string path, PriceCategory expectedCategory, VariableType expectedVariableType, IReadOnlyDictionary<string, PriceItem> priceItems, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            errors.Add($"{path}.PriceItemCode is required.");
            return null;
        }

        if (!priceItems.TryGetValue(code, out var item))
        {
            errors.Add($"{path}.PriceItemCode '{code}' was not found.");
            return null;
        }

        if (!item.IsActive)
        {
            errors.Add($"{path}.PriceItemCode '{code}' is inactive.");
            return null;
        }

        if (item.Category != expectedCategory)
            errors.Add($"{path}.PriceItemCode '{code}' must be in category {expectedCategory}.");

        if (item.VariableType != expectedVariableType)
            errors.Add($"{path}.PriceItemCode '{code}' must have variable type {expectedVariableType}.");

        return item.Category == expectedCategory && item.VariableType == expectedVariableType ? item : null;
    }

    private static bool ValidatePricingMode(PricingModeDto? pricingMode, string path, List<string> errors)
    {
        if (pricingMode != null)
            return true;

        errors.Add($"{path}.PricingMode is required when the item is enabled.");
        return false;
    }

    private static void AddCustomLine(decimal? customPrice, string path, ReportCategoryDto category, PriceItem item, List<ProjectPriceLineDto> lines, List<string> errors, ref int sortOrder, string? selection = null)
    {
        if (!HasPositiveValue(customPrice))
        {
            errors.Add($"{path}.CustomPrice must be greater than zero in custom mode.");
            return;
        }

        var amount = RoundMoney(customPrice!.Value);
        lines.Add(new ProjectPriceLineDto
        {
            Category = category,
            ItemCode = item.Code,
            ItemName = item.Name,
            PricingMode = PricingModeDto.Custom,
            Selection = selection ?? item.Name,
            Unit = "manual",
            RateUsed = null,
            CustomPrice = amount,
            CalculationText = "Manually entered price",
            FinalAmount = amount,
            SortOrder = NextSortOrder(ref sortOrder)
        });
    }

    private static void AddCalculatedLine(List<ProjectPriceLineDto> lines, ReportCategoryDto category, PriceItem item, int? quantity, decimal? area, decimal? length, decimal amount, string calculationText, ref int sortOrder, string? selection = null)
    {
        var roundedAmount = RoundMoney(amount);
        lines.Add(new ProjectPriceLineDto
        {
            Category = category,
            ItemCode = item.Code,
            ItemName = item.Name,
            PricingMode = PricingModeDto.Calculated,
            Selection = selection ?? item.Name,
            Quantity = quantity,
            Area = area,
            Length = length,
            Unit = item.Unit,
            RateUsed = RoundMoney(item.Rate),
            CustomPrice = null,
            CalculationText = calculationText,
            FinalAmount = roundedAmount,
            SortOrder = NextSortOrder(ref sortOrder)
        });
    }

    private static int NextSortOrder(ref int sortOrder)
    {
        var current = sortOrder;
        sortOrder += 10;
        return current;
    }

    private static bool HasPositiveValue(decimal? value)
    {
        return value > 0;
    }

    private static bool HasPositiveValue(int? value)
    {
        return value > 0;
    }

    private static decimal RoundMoney(decimal value)
    {
        return decimal.Round(value, 2, MidpointRounding.AwayFromZero);
    }

    private static string FormatMoney(decimal value)
    {
        return RoundMoney(value).ToString("0.00");
    }

    private static string FormatMeasure(decimal? value)
    {
        return value.GetValueOrDefault().ToString("0.##");
    }

    private static int CategorySortOrder(ReportCategoryDto category)
    {
        return category switch
        {
            ReportCategoryDto.Design => 10,
            ReportCategoryDto.Ceiling => 20,
            ReportCategoryDto.Walls => 30,
            ReportCategoryDto.Flooring => 40,
            ReportCategoryDto.Furnishing => 50,
            ReportCategoryDto.Other => 60,
            _ => 100
        };
    }
}
