namespace InteriorCalculator.Api.Models;

public class PriceItem
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public PriceCategory Category { get; set; }

    public decimal Rate { get; set; }

    public string Unit { get; set; } = string.Empty;

    public VariableType VariableType { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
