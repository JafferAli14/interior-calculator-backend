using System.ComponentModel.DataAnnotations;

namespace InteriorCalculator.Api.Models;

public class AuditLog
{
    public int Id { get; set; }

    public int ActorAdminId { get; set; }

    [Required]
    [MaxLength(150)]
    public string ActorUsername { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ActorFullName { get; set; }

    [Required]
    [MaxLength(20)]
    public string ActorRole { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string EntityType { get; set; } = string.Empty;

    public int EntityId { get; set; }

    [Required]
    [MaxLength(50)]
    public string EntityCode { get; set; } = string.Empty;

    [Required]
    public string OldValuesJson { get; set; } = string.Empty;

    [Required]
    public string NewValuesJson { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
