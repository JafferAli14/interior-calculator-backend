using System.ComponentModel.DataAnnotations;

namespace InteriorCalculator.Api.DTOs;

public class UpdateProjectDto
{
    [Required]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    public string ClientName { get; set; } = string.Empty;

    public string? ClientMobile { get; set; }

    [Required]
    public string ConfigurationJson { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }
}