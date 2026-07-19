using System.ComponentModel.DataAnnotations;

namespace InteriorCalculator.Api.Models;

public class ProjectEstimateLine
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public Project Project { get; set; } = null!;

    [MaxLength(50)]
    public string? PriceItemCode { get; set; }

    [Required]
    [MaxLength(150)]
    public string ItemName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string PricingMode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Selection { get; set; }

    public int? Quantity { get; set; }

    public decimal? Area { get; set; }

    public decimal? Length { get; set; }

    [Required]
    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    public decimal? Rate { get; set; }

    public decimal? CustomPrice { get; set; }

    [Required]
    [MaxLength(300)]
    public string Calculation { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int SortOrder { get; set; }
}
