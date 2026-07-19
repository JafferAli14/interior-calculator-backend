using System.Text.Json;

namespace InteriorCalculator.Api.DTOs;

public class SavedProjectSummaryDto
{
    public int Id { get; set; }

    public string ProjectNumber { get; set; } = string.Empty;

    public string ProjectName { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }

    public string? CustomerEmail { get; set; }

    public string? CustomerAddress { get; set; }

    public string Status { get; set; } = string.Empty;

    public string Currency { get; set; } = string.Empty;

    public decimal GrandTotal { get; set; }

    public string CreatedByUsername { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class SavedProjectResponseDto : SavedProjectSummaryDto
{
    public JsonElement PlannerRequest { get; set; }

    public List<CategorySubtotalDto> CategorySubtotals { get; set; } = [];

    public List<ProjectPriceLineDto> PriceLines { get; set; } = [];

    public List<string> Warnings { get; set; } = [];

    public int CreatedByAdminId { get; set; }

    public string? CreatedByFullName { get; set; }

    public DateTime UpdatedAt { get; set; }
}
