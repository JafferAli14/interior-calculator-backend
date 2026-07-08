using InteriorCalculator.Api.Models;

namespace InteriorCalculator.Api.DTOs;

public class PriceItemResponseDto
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public PriceCategory Category { get; set; }

    public decimal Rate { get; set; }

    public string Unit { get; set; } = string.Empty;

    public VariableType VariableType { get; set; }
}
