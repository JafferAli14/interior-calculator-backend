using System.ComponentModel.DataAnnotations;

namespace InteriorCalculator.Api.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    [MaxLength(30)]
    public string ProjectNumber { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? CustomerPhone { get; set; }

    [MaxLength(150)]
    public string? CustomerEmail { get; set; }

    [MaxLength(200)]
    public string? CustomerAddress { get; set; }

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Saved";

    [Required]
    [MaxLength(10)]
    public string Currency { get; set; } = string.Empty;

    public decimal GrandTotal { get; set; }

    [Required]
    public string PlannerRequestJson { get; set; } = string.Empty;

    [Required]
    public string CategorySubtotalsJson { get; set; } = string.Empty;

    [Required]
    public string WarningsJson { get; set; } = string.Empty;

    public int CreatedByAdminId { get; set; }

    [Required]
    [MaxLength(150)]
    public string CreatedByUsername { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? CreatedByFullName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<ProjectEstimateLine> EstimateLines { get; set; } = [];
}
