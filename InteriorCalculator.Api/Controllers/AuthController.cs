using System.Security.Claims;
using InteriorCalculator.Api.DTOs;
using InteriorCalculator.Api.Models;
using InteriorCalculator.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteriorCalculator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var adminIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(adminIdValue, out var adminId))
            return Unauthorized(new { message = "Invalid token" });

        var admin = await _authService.GetActiveAdminById(adminId);

        if (admin == null)
            return Unauthorized(new { message = "Admin account not found or inactive" });

        return Ok(new
        {
            message = "Authenticated successfully",
            admin = new
            {
                admin.Id,
                admin.FullName,
                admin.Username,
                Role = admin.Role.ToString()
            }
        });
    }

    [Authorize(Roles = nameof(AdminRole.SuperAdmin))]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterAdminDto dto)
    {
        try
        {
            var result = await _authService.Register(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        try
        {
            var result = await _authService.Login(dto);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }
}
