namespace InteriorCalculator.Api.DTOs;

public sealed class BedroomPreviewResponseDto
{
    public string SchemaVersion { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public List<ProjectPriceLineDto> PriceLines { get; init; } = [];
    public List<CategorySubtotalDto> CategorySubtotals { get; init; } = [];
    public decimal GrandTotal { get; init; }
    public List<string> Warnings { get; init; } = [];
}

public sealed class ProjectPriceLineDto
{
    public ReportCategoryDto Category { get; init; }
    public string? ItemCode { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public PricingModeDto PricingMode { get; init; }
    public string? Selection { get; init; }
    public int? Quantity { get; init; }
    public decimal? Area { get; init; }
    public decimal? Length { get; init; }
    public string Unit { get; init; } = string.Empty;
    public decimal? RateUsed { get; init; }
    public decimal? CustomPrice { get; init; }
    public string CalculationText { get; init; } = string.Empty;
    public decimal FinalAmount { get; init; }
    public int SortOrder { get; init; }
}

public sealed class CategorySubtotalDto
{
    public ReportCategoryDto Category { get; init; }
    public decimal Amount { get; init; }
}
