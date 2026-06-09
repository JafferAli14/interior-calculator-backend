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

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectDto dto)
    {
        var project = await _projectService.Create(dto);
        return Ok(project);
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
        var project = await _projectService.GetById(id);

        if (project == null)
            return NotFound(new { message = "Project not found" });

        return Ok(project);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProjectDto dto)
    {
        var project = await _projectService.Update(id, dto);

        if (project == null)
            return NotFound(new { message = "Project not found" });

        return Ok(project);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _projectService.Delete(id);

        if (!deleted)
            return NotFound(new { message = "Project not found" });

        return Ok(new { message = "Project deleted successfully" });
    }
}