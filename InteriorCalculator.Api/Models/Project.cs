using System.ComponentModel.DataAnnotations;

namespace InteriorCalculator.Api.Models;

public class Project
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string ProjectName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string ClientName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? ClientMobile { get; set; }

    [Required]
    public string ConfigurationJson { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}