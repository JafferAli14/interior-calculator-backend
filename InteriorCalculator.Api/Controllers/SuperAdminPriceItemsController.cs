using System.Security.Claims;
using InteriorCalculator.Api.DTOs;
using InteriorCalculator.Api.Models;
using InteriorCalculator.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteriorCalculator.Api.Controllers;

[ApiController]
[Route("api/superadmin/price-items")]
[Authorize(Roles = nameof(AdminRole.SuperAdmin))]
public class SuperAdminPriceItemsController : ControllerBase
{
    private readonly PriceItemService _priceItemService;

    public SuperAdminPriceItemsController(PriceItemService priceItemService)
    {
        _priceItemService = priceItemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllForManagement()
    {
        var priceItems = await _priceItemService.GetAllForManagement();
        return Ok(priceItems);
    }

    [HttpPatch("{id:int}/rate")]
    public async Task<IActionResult> UpdateRate(int id, UpdatePriceItemRateDto dto)
    {
        if (!TryGetActorAdminId(out var actorAdminId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var priceItem = await _priceItemService.UpdateRate(id, dto.Rate, actorAdminId);

            if (priceItem == null)
                return NotFound(new { message = "Price item not found" });

            return Ok(priceItem);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdatePriceItemStatusDto dto)
    {
        if (!TryGetActorAdminId(out var actorAdminId))
            return Unauthorized(new { message = "Invalid token" });

        try
        {
            var priceItem = await _priceItemService.UpdateStatus(id, dto.IsActive, actorAdminId);

            if (priceItem == null)
                return NotFound(new { message = "Price item not found" });

            return Ok(priceItem);
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    private bool TryGetActorAdminId(out int adminId)
    {
        var adminIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(adminIdValue, out adminId);
    }
}
