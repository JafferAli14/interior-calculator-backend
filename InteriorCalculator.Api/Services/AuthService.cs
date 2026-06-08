using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using InteriorCalculator.Api.Data;
using InteriorCalculator.Api.DTOs;
using InteriorCalculator.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace InteriorCalculator.Api.Services;

public class AuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<object> Register(RegisterAdminDto dto)
    {
        var existingAdmin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Username == dto.Username);

        if (existingAdmin != null)
        {
            throw new Exception("Username already exists");
        }

        var admin = new Admin
        {
            FullName = dto.FullName,
            Username = dto.Username,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "Admin"
        };

        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();

        return new
        {
            message = "Admin registered successfully",
            admin = new
            {
                admin.Id,
                admin.FullName,
                admin.Username,
                admin.Role
            }
        };
    }

    public async Task<object> Login(LoginDto dto)
    {
        var admin = await _context.Admins
            .FirstOrDefaultAsync(a => a.Username == dto.Username);

        if (admin == null)
        {
            throw new Exception("Invalid username or password");
        }

        var isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash);

        if (!isPasswordValid)
        {
            throw new Exception("Invalid username or password");
        }

        if (!admin.IsActive)
        {
            throw new Exception("Admin account is inactive");
        }

        var token = GenerateJwtToken(admin);

        return new
        {
            message = "Login successful",
            token,
            admin = new
            {
                admin.Id,
                admin.FullName,
                admin.Username,
                admin.Role
            }
        };
    }

    private string GenerateJwtToken(Admin admin)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, admin.Role)
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}