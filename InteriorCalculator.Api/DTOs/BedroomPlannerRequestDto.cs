using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace InteriorCalculator.Api.DTOs;

public sealed class BedroomPlannerRequestDto
{
    [Required]
    public string SchemaVersion { get; init; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string ProjectName { get; init; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ClientName { get; init; } = string.Empty;

    [MaxLength(20)]
    public string? ClientMobile { get; init; }

    [Required]
    public string Currency { get; init; } = "QAR";

    [Required]
    public BedroomMeasurementsDto Measurements { get; init; } = new();

    [Required]
    public BedroomDesignDto Design { get; init; } = new();

    [Required]
    public BedroomCeilingDto Ceiling { get; init; } = new();

    [Required]
    public BedroomWallsDto Walls { get; init; } = new();

    [Required]
    public BedroomFlooringDto Flooring { get; init; } = new();

    [Required]
    public BedroomFurnishingDto Furnishing { get; init; } = new();

    public List<AdditionalRequirementDto> AdditionalRequirements { get; init; } = [];
}

public sealed class BedroomMeasurementsDto
{
    public decimal? RoomLength { get; init; }
    public decimal? RoomWidth { get; init; }
    public decimal? CeilingArea { get; init; }
    public decimal? WallArea { get; init; }
    public decimal? FlooringArea { get; init; }
}

public sealed class BedroomDesignDto
{
    public string? PriceItemCode { get; init; }
}

public sealed class BedroomCeilingDto
{
    public EnabledCodeItemDto? GypsumCeiling { get; init; }
    public LengthItemDto? Cornish { get; init; }
    public List<QuantityCodeItemDto> CeilingLights { get; init; } = [];
    public CustomQuantityItemDto? Chandelier { get; init; }
    public LengthItemDto? CurtainBox { get; init; }
    public PaintCodeItemDto? CeilingPainting { get; init; }
}

public sealed class BedroomWallsDto
{
    public LengthItemDto? Curtain { get; init; }
    public LengthItemDto? Moulding { get; init; }
    public CustomPaintItemDto? WallPainting { get; init; }
    public EnabledCodeItemDto? Wallpaper { get; init; }
    public QuantityCodeItemDto? Doors { get; init; }
    public QuantityCodeItemDto? Windows { get; init; }
    public AreaItemDto? Cladding { get; init; }
}

public sealed class BedroomFlooringDto
{
    public TileItemDto? Tiles { get; init; }
    public LengthItemDto? Skirting { get; init; }
    public AreaItemDto? Parquet { get; init; }
    public AreaItemDto? Glasswork { get; init; }
}

public sealed class BedroomFurnishingDto
{
    public EnabledCodeItemDto? Bed { get; init; }
    public CustomAreaItemDto? HeadboardCladding { get; init; }
    public QuantityCodeItemDto? SideTable { get; init; }
    public QuantityCodeItemDto? SideLamps { get; init; }
    public CustomFixedItemDto? TvUnit { get; init; }
    public QuantityCodeItemDto? Chairs { get; init; }
    public QuantityCodeItemDto? Stools { get; init; }
    public EnabledCodeItemDto? DressingTable { get; init; }
    public AreaItemDto? Carpet { get; init; }
    public EnabledCodeItemDto? Bench { get; init; }
    public CustomQuantityItemDto? Ac { get; init; }
}

public sealed class EnabledCodeItemDto
{
    public bool Enabled { get; init; }
    public string? PriceItemCode { get; init; }
}

public sealed class QuantityCodeItemDto
{
    public bool Enabled { get; init; } = true;
    public string? PriceItemCode { get; init; }
    public int? Quantity { get; init; }
}

public sealed class AreaItemDto
{
    public bool Enabled { get; init; }
    public string? PriceItemCode { get; init; }
    public decimal? Area { get; init; }
}

public sealed class LengthItemDto
{
    public bool Enabled { get; init; }
    public string? PriceItemCode { get; init; }
    public decimal? Length { get; init; }
}

public sealed class PaintCodeItemDto
{
    public bool Enabled { get; init; }
    public string? PriceItemCode { get; init; }
    public string? PaintColour { get; init; }
}

public sealed class TileItemDto
{
    public bool Enabled { get; init; }
    public string? PriceItemCode { get; init; }
    public string? Material { get; init; }
    public string? TileSize { get; init; }
}

public sealed class CustomQuantityItemDto
{
    public bool Enabled { get; init; }
    public string? PriceItemCode { get; init; }
    public PricingModeDto? PricingMode { get; init; }
    public int? Quantity { get; init; }
    public decimal? CustomPrice { get; init; }
}

public sealed class CustomAreaItemDto
{
    public bool Enabled { get; init; }
    public string? PriceItemCode { get; init; }
    public PricingModeDto? PricingMode { get; init; }
    public decimal? Area { get; init; }
    public decimal? CustomPrice { get; init; }
}

public sealed class CustomPaintItemDto
{
    public bool Enabled { get; init; }
    public string? PriceItemCode { get; init; }
    public PricingModeDto? PricingMode { get; init; }
    public decimal? CustomPrice { get; init; }
    public string? PaintColour { get; init; }
}

public sealed class CustomFixedItemDto
{
    public bool Enabled { get; init; }
    public string? PriceItemCode { get; init; }
    public PricingModeDto? PricingMode { get; init; }
    public decimal? CustomPrice { get; init; }
}

public sealed class AdditionalRequirementDto
{
    public ReportCategoryDto Category { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public decimal CustomPrice { get; init; }
    public int SortOrder { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PricingModeDto
{
    Calculated,
    Custom
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReportCategoryDto
{
    Design,
    Ceiling,
    Walls,
    Flooring,
    Furnishing,
    Other
}
