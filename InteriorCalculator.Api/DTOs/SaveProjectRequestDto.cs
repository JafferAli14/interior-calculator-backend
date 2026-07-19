using System.ComponentModel.DataAnnotations;

namespace InteriorCalculator.Api.DTOs;

public class SaveProjectRequestDto
{
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
    public BedroomPlannerRequestDto PlannerRequest { get; set; } = new();
}
