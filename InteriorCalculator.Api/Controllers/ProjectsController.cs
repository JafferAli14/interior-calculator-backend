using InteriorCalculator.Api.DTOs;
using InteriorCalculator.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InteriorCalculator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly ProjectService _projectService;
    private readonly BedroomPricingService _bedroomPricingService;

    public ProjectsController(
        ProjectService projectService,
        BedroomPricingService bedroomPricingService)
    {
        _projectService = projectService;
        _bedroomPricingService = bedroomPricingService;
    }

    [HttpPost("preview")]
    public async Task<IActionResult> Preview(BedroomPlannerRequestDto dto)
    {
        try
        {
            var preview = await _bedroomPricingService.PreviewAsync(dto);
            return Ok(preview);
        }
        catch (BedroomPricingValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(SaveProjectRequestDto dto)
    {
        try
        {
            var project = await _projectService.Save(dto, User);
            return Ok(project);
        }
        catch (SaveProjectValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (BedroomPricingValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (ProjectSnapshotReadException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var projects = await _projectService.GetAll();
        return Ok(projects);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var project = await _projectService.GetById(id);

            if (project == null)
                return NotFound(new { message = "Project not found" });

            return Ok(project);
        }
        catch (ProjectSnapshotReadException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
        }
    }

}
