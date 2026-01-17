using Crypto_Payment.Data;
using Crypto_Payment.DTOS;
using Crypto_Payment.Models;
using Crypto_Payment.Services;
using Microsoft.EntityFrameworkCore;

public class PermissionManager : IPermissionService
{
    private readonly AppDbContext _db;

    public PermissionManager(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PermissionDto> CreateAsync(PermissionDto dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));
        
        var permission = new Permission()
        {
            Name = dto.Name.Trim(),
            NormalizedName = dto.Name.Trim().ToUpperInvariant()
        };

        _db.Permissions.Add(permission);
        await _db.SaveChangesAsync();

        dto.Id = permission.Id;
        return dto;
    }

    public async Task<PermissionDto> UpdateAsync(int id, PermissionDto dto)
    {
        var permission = await _db.Permissions.FirstOrDefaultAsync(x => x.Id == id);
        if (permission == null) throw new KeyNotFoundException("Yetki bulunamadı.");

        permission.Name = dto.Name.Trim();
        permission.NormalizedName = dto.Name.Trim().ToUpperInvariant();

        await _db.SaveChangesAsync();

        dto.Id = permission.Id;
        return dto;
    }

    public async Task<List<PermissionDto>> GetAllAsync()
    {
        return await _db.Permissions
            .Select(x => new PermissionDto()
            { 
                Id = x.Id,
                Name = x.Name,
                TopPermissionId = x.TopPermissionId,
                TopPermissionName = _db.Permissions
                    .Where(p => p.Id == x.TopPermissionId)
                    .Select(p => p.Name)
                    .FirstOrDefault()
            })
            .ToListAsync();
    }

    public async Task<PermissionDto> GetByIdAsync(int id)
    {
        var x = await _db.Permissions.FirstOrDefaultAsync(c => c.Id == id);
        if (x == null) throw new KeyNotFoundException("Yetki bulunamadı.");

        return new PermissionDto()
        {
            Id = x.Id,
            Name = x.Name,
            TopPermissionId = x.TopPermissionId,
            TopPermissionName = _db.Permissions
                .Where(p => p.Id == x.TopPermissionId)
                .Select(p => p.Name)
                .FirstOrDefault()
        };
    }

    public async Task DeleteAsync(int id)
    {
        var permission = await _db.Permissions.FirstOrDefaultAsync(x => x.Id == id);
        if (permission == null) throw new KeyNotFoundException("Yetki bulunamadı.");

        _db.Permissions.Remove(permission);
        await _db.SaveChangesAsync();
    }
}