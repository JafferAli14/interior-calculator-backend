using InteriorCalculator.Api.Data;
using InteriorCalculator.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace InteriorCalculator.Api.Services;

public class PriceItemService
{
    private readonly AppDbContext _context;

    public PriceItemService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PriceItemResponseDto>> GetAllActive()
    {
        return await _context.PriceItems
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(p => new PriceItemResponseDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Category = p.Category,
                Rate = p.Rate,
                Unit = p.Unit,
                VariableType = p.VariableType
            })
            .ToListAsync();
    }

    public async Task<PriceItemResponseDto?> GetActiveByCode(string code)
    {
        return await _context.PriceItems
            .AsNoTracking()
            .Where(p => p.IsActive && p.Code == code)
            .Select(p => new PriceItemResponseDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Category = p.Category,
                Rate = p.Rate,
                Unit = p.Unit,
                VariableType = p.VariableType
            })
            .FirstOrDefaultAsync();
    }
}
