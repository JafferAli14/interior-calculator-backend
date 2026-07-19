using System.Security.Claims;
using System.Text.Json;
using InteriorCalculator.Api.Data;
using InteriorCalculator.Api.DTOs;
using InteriorCalculator.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InteriorCalculator.Api.Services;

public sealed class SaveProjectValidationException : Exception
{
    public SaveProjectValidationException(List<string> errors)
        : base("Project save request validation failed.")
    {
        Errors = errors;
    }

    public List<string> Errors { get; }
}

public sealed class ProjectSnapshotReadException : Exception
{
    public ProjectSnapshotReadException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

public class ProjectService
{
    private const int MaxProjectNumberGenerationAttempts = 5;
    private const string SavedStatus = "Saved";
    private readonly AppDbContext _context;
    private readonly BedroomPricingService _bedroomPricingService;

    public ProjectService(AppDbContext context, BedroomPricingService bedroomPricingService)
    {
        _context = context;
        _bedroomPricingService = bedroomPricingService;
    }

    public async Task<SavedProjectResponseDto> Save(SaveProjectRequestDto dto, ClaimsPrincipal user)
    {
        ValidateSaveRequest(dto);

        var actor = await GetActiveActor(user);
        var normalizedProjectName = dto.ProjectName.Trim();
        var normalizedCustomerName = dto.CustomerName.Trim();
        var normalizedCustomerPhone = NormalizeOptional(dto.CustomerPhone);
        var normalizedCustomerEmail = NormalizeOptional(dto.CustomerEmail);
        var normalizedCustomerAddress = NormalizeOptional(dto.CustomerAddress);
        var plannerRequest = CopyMetadataToPlannerRequest(
            dto.PlannerRequest,
            normalizedProjectName,
            normalizedCustomerName,
            normalizedCustomerPhone);
        var preview = await _bedroomPricingService.PreviewAsync(plannerRequest);
        var now = DateTime.UtcNow;

        for (var attempt = 1; attempt <= MaxProjectNumberGenerationAttempts; attempt++)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var project = new Project
            {
                ProjectNumber = await GenerateProjectNumber(now),
                ProjectName = normalizedProjectName,
                CustomerName = normalizedCustomerName,
                CustomerPhone = normalizedCustomerPhone,
                CustomerEmail = normalizedCustomerEmail,
                CustomerAddress = normalizedCustomerAddress,
                Status = SavedStatus,
                Currency = preview.Currency,
                GrandTotal = preview.GrandTotal,
                PlannerRequestJson = JsonSerializer.Serialize(plannerRequest),
                CategorySubtotalsJson = JsonSerializer.Serialize(preview.CategorySubtotals),
                WarningsJson = JsonSerializer.Serialize(preview.Warnings),
                CreatedByAdminId = actor.Id,
                CreatedByUsername = actor.Username,
                CreatedByFullName = actor.FullName,
                CreatedAt = now,
                UpdatedAt = now,
                EstimateLines = preview.PriceLines
                    .Select(line => new ProjectEstimateLine
                    {
                        PriceItemCode = line.ItemCode,
                        ItemName = line.ItemName,
                        Category = line.Category.ToString(),
                        PricingMode = line.PricingMode.ToString(),
                        Selection = line.Selection,
                        Quantity = line.Quantity,
                        Area = line.Area,
                        Length = line.Length,
                        Unit = line.Unit,
                        Rate = line.RateUsed,
                        CustomPrice = line.CustomPrice,
                        Calculation = line.CalculationText,
                        Amount = line.FinalAmount,
                        SortOrder = line.SortOrder
                    })
                    .ToList()
            };

            try
            {
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ToSavedProjectResponse(project);
            }
            catch (DbUpdateException ex) when (IsProjectNumberConflict(ex))
            {
                await transaction.RollbackAsync();
                _context.ChangeTracker.Clear();

                if (attempt == MaxProjectNumberGenerationAttempts)
                {
                    throw new InvalidOperationException(
                        "Could not generate a unique project number. Please try saving again.");
                }
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        throw new InvalidOperationException("Could not generate a unique project number. Please try saving again.");
    }

    public async Task<List<SavedProjectSummaryDto>> GetAll()
    {
        return await _context.Projects
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new SavedProjectSummaryDto
            {
                Id = p.Id,
                ProjectNumber = p.ProjectNumber,
                ProjectName = p.ProjectName,
                CustomerName = p.CustomerName,
                CustomerPhone = p.CustomerPhone,
                CustomerEmail = p.CustomerEmail,
                CustomerAddress = p.CustomerAddress,
                Status = p.Status,
                Currency = p.Currency,
                GrandTotal = p.GrandTotal,
                CreatedByUsername = p.CreatedByUsername,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<SavedProjectResponseDto?> GetById(int id)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .Include(p => p.EstimateLines)
            .FirstOrDefaultAsync(p => p.Id == id);

        return project == null ? null : ToSavedProjectResponse(project);
    }

    private static void ValidateSaveRequest(SaveProjectRequestDto dto)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.ProjectName))
            errors.Add("ProjectName is required.");
        else if (dto.ProjectName.Length > 150)
            errors.Add("ProjectName must be 150 characters or fewer.");

        if (string.IsNullOrWhiteSpace(dto.CustomerName))
            errors.Add("CustomerName is required.");
        else if (dto.CustomerName.Length > 100)
            errors.Add("CustomerName must be 100 characters or fewer.");

        if (dto.CustomerPhone?.Length > 20)
            errors.Add("CustomerPhone must be 20 characters or fewer.");

        if (dto.CustomerEmail?.Length > 150)
            errors.Add("CustomerEmail must be 150 characters or fewer.");

        if (dto.CustomerAddress?.Length > 200)
            errors.Add("CustomerAddress must be 200 characters or fewer.");

        if (dto.PlannerRequest == null)
            errors.Add("PlannerRequest is required.");

        if (errors.Count > 0)
            throw new SaveProjectValidationException(errors);
    }

    private async Task<Admin> GetActiveActor(ClaimsPrincipal user)
    {
        var adminIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(adminIdValue, out var adminId))
            throw new InvalidOperationException("Invalid token.");

        var actor = await _context.Admins
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == adminId && a.IsActive);

        if (actor == null)
            throw new InvalidOperationException("Admin account not found or inactive.");

        return actor;
    }

    private async Task<string> GenerateProjectNumber(DateTime now)
    {
        var prefix = $"PRJ-{now:yyyyMMdd}";
        var lastProjectNumber = await _context.Projects
            .Where(p => p.ProjectNumber.StartsWith(prefix))
            .OrderByDescending(p => p.ProjectNumber)
            .Select(p => p.ProjectNumber)
            .FirstOrDefaultAsync();

        var nextNumber = 1;

        if (!string.IsNullOrWhiteSpace(lastProjectNumber))
        {
            var suffix = lastProjectNumber[(prefix.Length + 1)..];

            if (int.TryParse(suffix, out var lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}-{nextNumber:0000}";
    }

    private static BedroomPlannerRequestDto CopyMetadataToPlannerRequest(
        BedroomPlannerRequestDto plannerRequest,
        string projectName,
        string customerName,
        string? customerPhone)
    {
        return new BedroomPlannerRequestDto
        {
            SchemaVersion = plannerRequest.SchemaVersion,
            ProjectName = projectName,
            ClientName = customerName,
            ClientMobile = customerPhone,
            Currency = plannerRequest.Currency,
            Measurements = plannerRequest.Measurements,
            Design = plannerRequest.Design,
            Ceiling = plannerRequest.Ceiling,
            Walls = plannerRequest.Walls,
            Flooring = plannerRequest.Flooring,
            Furnishing = plannerRequest.Furnishing,
            AdditionalRequirements = plannerRequest.AdditionalRequirements
        };
    }

    private static bool IsProjectNumberConflict(DbUpdateException exception)
    {
        var message = exception.ToString();

        return message.Contains("ProjectNumber", StringComparison.OrdinalIgnoreCase)
            || message.Contains("IX_Projects_ProjectNumber", StringComparison.OrdinalIgnoreCase);
    }

    private static SavedProjectResponseDto ToSavedProjectResponse(Project project)
    {
        try
        {
            return new SavedProjectResponseDto
            {
                Id = project.Id,
                ProjectNumber = project.ProjectNumber,
                ProjectName = project.ProjectName,
                CustomerName = project.CustomerName,
                CustomerPhone = project.CustomerPhone,
                CustomerEmail = project.CustomerEmail,
                CustomerAddress = project.CustomerAddress,
                Status = project.Status,
                Currency = project.Currency,
                GrandTotal = project.GrandTotal,
                PlannerRequest = ParseJsonElement(project.PlannerRequestJson),
                CategorySubtotals = DeserializeJson<List<CategorySubtotalDto>>(project.CategorySubtotalsJson),
                PriceLines = project.EstimateLines
                    .OrderBy(line => line.SortOrder)
                    .Select(line => new ProjectPriceLineDto
                    {
                        Category = Enum.Parse<ReportCategoryDto>(line.Category),
                        ItemCode = line.PriceItemCode,
                        ItemName = line.ItemName,
                        PricingMode = Enum.Parse<PricingModeDto>(line.PricingMode),
                        Selection = line.Selection,
                        Quantity = line.Quantity,
                        Area = line.Area,
                        Length = line.Length,
                        Unit = line.Unit,
                        RateUsed = line.Rate,
                        CustomPrice = line.CustomPrice,
                        CalculationText = line.Calculation,
                        FinalAmount = line.Amount,
                        SortOrder = line.SortOrder
                    })
                    .ToList(),
                Warnings = DeserializeJson<List<string>>(project.WarningsJson),
                CreatedByAdminId = project.CreatedByAdminId,
                CreatedByUsername = project.CreatedByUsername,
                CreatedByFullName = project.CreatedByFullName,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            throw new ProjectSnapshotReadException("Saved project snapshot data is malformed.", ex);
        }
    }

    private static JsonElement ParseJsonElement(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private static T DeserializeJson<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json) ?? Activator.CreateInstance<T>();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
