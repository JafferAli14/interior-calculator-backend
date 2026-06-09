using InteriorCalculator.Api.Data;
using InteriorCalculator.Api.DTOs;
using InteriorCalculator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InteriorCalculator.Api.Services;

public class ProjectService
{
    private readonly AppDbContext _context;

    public ProjectService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Project> Create(CreateProjectDto dto)
    {
        var project = new Project
        {
            ProjectName = dto.ProjectName,
            ClientName = dto.ClientName,
            ClientMobile = dto.ClientMobile,
            ConfigurationJson = dto.ConfigurationJson,
            TotalAmount = dto.TotalAmount,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Projects.Add(project);
        await _context.SaveChangesAsync();

        return project;
    }

    public async Task<List<Project>> GetAll()
    {
        return await _context.Projects
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Project?> GetById(int id)
    {
        return await _context.Projects.FindAsync(id);
    }

    public async Task<Project?> Update(int id, UpdateProjectDto dto)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
            return null;

        project.ProjectName = dto.ProjectName;
        project.ClientName = dto.ClientName;
        project.ClientMobile = dto.ClientMobile;
        project.ConfigurationJson = dto.ConfigurationJson;
        project.TotalAmount = dto.TotalAmount;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return project;
    }

    public async Task<bool> Delete(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null)
            return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();

        return true;
    }
}