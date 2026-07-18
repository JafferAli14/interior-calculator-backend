using System.Text.Json;
using InteriorCalculator.Api.Data;
using InteriorCalculator.Api.DTOs;
using InteriorCalculator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InteriorCalculator.Api.Services;

public class PriceItemService
{
    private const string PriceItemEntityType = "PriceItem";
    private const string PriceItemRateUpdatedAction = "PriceItemRateUpdated";
    private const string PriceItemStatusUpdatedAction = "PriceItemStatusUpdated";
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

    public async Task<List<PriceItemManagementResponseDto>> GetAllForManagement()
    {
        return await _context.PriceItems
            .AsNoTracking()
            .OrderBy(p => p.Category)
            .ThenBy(p => p.Name)
            .Select(p => new PriceItemManagementResponseDto
            {
                Id = p.Id,
                Code = p.Code,
                Name = p.Name,
                Category = p.Category,
                Rate = p.Rate,
                Unit = p.Unit,
                VariableType = p.VariableType,
                IsActive = p.IsActive,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task<PriceItemManagementResponseDto?> UpdateRate(
        int id,
        decimal rate,
        int actorAdminId)
    {
        if (rate < 0)
            throw new ArgumentException("Rate cannot be negative.", nameof(rate));

        var priceItem = await _context.PriceItems
            .FirstOrDefaultAsync(p => p.Id == id);

        if (priceItem == null)
            return null;

        var actor = await GetActiveActor(actorAdminId);
        var oldRate = priceItem.Rate;

        priceItem.Rate = decimal.Round(rate, 2, MidpointRounding.AwayFromZero);
        priceItem.UpdatedAt = DateTime.UtcNow;

        AddAuditLog(
            actor,
            PriceItemRateUpdatedAction,
            priceItem,
            new { Rate = oldRate },
            new { priceItem.Rate });

        await _context.SaveChangesAsync();

        return ToManagementDto(priceItem);
    }

    public async Task<PriceItemManagementResponseDto?> UpdateStatus(
        int id,
        bool isActive,
        int actorAdminId)
    {
        var priceItem = await _context.PriceItems
            .FirstOrDefaultAsync(p => p.Id == id);

        if (priceItem == null)
            return null;

        var actor = await GetActiveActor(actorAdminId);
        var oldIsActive = priceItem.IsActive;

        priceItem.IsActive = isActive;
        priceItem.UpdatedAt = DateTime.UtcNow;

        AddAuditLog(
            actor,
            PriceItemStatusUpdatedAction,
            priceItem,
            new { IsActive = oldIsActive },
            new { priceItem.IsActive });

        await _context.SaveChangesAsync();

        return ToManagementDto(priceItem);
    }

    private async Task<Admin> GetActiveActor(int actorAdminId)
    {
        var actor = await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == actorAdminId && a.IsActive);

        if (actor == null)
            throw new InvalidOperationException("Authenticated admin account was not found or is inactive.");

        return actor;
    }

    private void AddAuditLog(
        Admin actor,
        string action,
        PriceItem priceItem,
        object oldValues,
        object newValues)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            ActorAdminId = actor.Id,
            ActorUsername = actor.Username,
            ActorFullName = actor.FullName,
            ActorRole = actor.Role.ToString(),
            Action = action,
            EntityType = PriceItemEntityType,
            EntityId = priceItem.Id,
            EntityCode = priceItem.Code,
            OldValuesJson = JsonSerializer.Serialize(oldValues),
            NewValuesJson = JsonSerializer.Serialize(newValues),
            CreatedAt = DateTime.UtcNow
        });
    }

    private static PriceItemManagementResponseDto ToManagementDto(PriceItem priceItem)
    {
        return new PriceItemManagementResponseDto
        {
            Id = priceItem.Id,
            Code = priceItem.Code,
            Name = priceItem.Name,
            Category = priceItem.Category,
            Rate = priceItem.Rate,
            Unit = priceItem.Unit,
            VariableType = priceItem.VariableType,
            IsActive = priceItem.IsActive,
            CreatedAt = priceItem.CreatedAt,
            UpdatedAt = priceItem.UpdatedAt
        };
    }
}
