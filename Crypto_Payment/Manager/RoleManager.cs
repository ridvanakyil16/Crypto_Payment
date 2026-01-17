using Crypto_Payment.Data;
using Crypto_Payment.DTOS;
using Crypto_Payment.Models;
using Crypto_Payment.Services;
using Microsoft.EntityFrameworkCore;

public class RoleManager : IRoleService
{
    private readonly AppDbContext _db;

    public RoleManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RoleDto> CreateAsync(RoleDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        
        var role = new Role
        {
            Name = dto.Name.Trim(),
            NormalizedName = dto.Name.Trim().ToUpperInvariant()
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        dto.Id = role.Id;
        return dto;
    }

    public async Task<RoleDto> UpdateAsync(string id, RoleDto dto)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == id);
        if (role == null) throw new KeyNotFoundException("Rol bulunamadı.");

        role.Name = dto.Name.Trim();
        role.NormalizedName = dto.Name.Trim().ToUpperInvariant();

        await _db.SaveChangesAsync();

        dto.Id = role.Id;
        return dto;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        return await _db.Roles
            .Select(x => new RoleDto()
            { 
                Id = x.Id,
                Name = x.Name,
                NormalizedName = x.Name.ToUpperInvariant()
            })
            .ToListAsync();
    }

    public async Task<RoleDto> GetByIdAsync(string id)
    {
        var x = await _db.Roles.FirstOrDefaultAsync(c => c.Id == id);
        if (x == null) throw new KeyNotFoundException("Rol bulunamadı.");

        return new RoleDto()
        {
            Id = x.Id,
            Name = x.Name,
            NormalizedName = x.Name.ToUpperInvariant()
        };
    }

    public async Task DeleteAsync(string id)
    {
        var role = await _db.Roles.FirstOrDefaultAsync(x => x.Id == id);
        if (role == null) throw new KeyNotFoundException("Rol bulunamadı.");

        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();
    }
}