using InteriorCalculator.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteriorCalculator.Api.Controllers;

[ApiController]
[Route("api/price-items")]
[Authorize]
public class PriceItemsController : ControllerBase
{
    private readonly PriceItemService _priceItemService;

    public PriceItemsController(PriceItemService priceItemService)
    {
        _priceItemService = priceItemService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var priceItems = await _priceItemService.GetAllActive();
        return Ok(priceItems);
    }

    [HttpGet("{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var priceItem = await _priceItemService.GetActiveByCode(code);

        if (priceItem == null)
            return NotFound(new { message = "Price item not found" });

        return Ok(priceItem);
    }
}
