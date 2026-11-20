using Microsoft.EntityFrameworkCore;
using Sav.Application.Common;
using Sav.Domain.Brands;

namespace Sav.Application.Brands;

public interface IBrandService
{
    Task<IReadOnlyList<BrandDto>> ListAsync(string? search, CancellationToken ct);

    Task<BrandDto> GetAsync(int id, CancellationToken ct);

    Task<BrandDto> CreateAsync(SaveBrandRequest request, CancellationToken ct);

    Task<BrandDto> UpdateAsync(int id, SaveBrandRequest request, CancellationToken ct);

    Task DeactivateAsync(int id, CancellationToken ct);
}

public sealed class BrandService(IApplicationDbContext db, TimeProvider clock) : IBrandService
{
    // Small catalog (~70 rows in the original system): no paging, the UI loads it whole for dropdowns.
    public async Task<IReadOnlyList<BrandDto>> ListAsync(string? search, CancellationToken ct)
    {
        var query = db.Brands.AsNoTracking().Where(b => b.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(b => b.Name.Contains(term));
        }

        var brands = await query.OrderBy(b => b.Name).ToListAsync(ct);
        return brands.Select(BrandDto.From).ToList();
    }

    public async Task<BrandDto> GetAsync(int id, CancellationToken ct) =>
        BrandDto.From(await FindAsync(id, ct));

    public async Task<BrandDto> CreateAsync(SaveBrandRequest request, CancellationToken ct)
    {
        var brand = new Brand(request.Name, Now());
        await EnsureNameIsFreeAsync(brand.Name, exceptId: null, ct);

        db.Brands.Add(brand);
        await db.SaveChangesAsync(ct);
        return BrandDto.From(brand);
    }

    public async Task<BrandDto> UpdateAsync(int id, SaveBrandRequest request, CancellationToken ct)
    {
        var brand = await FindAsync(id, ct);
        brand.Rename(request.Name, Now());
        await EnsureNameIsFreeAsync(brand.Name, exceptId: id, ct);

        await db.SaveChangesAsync(ct);
        return BrandDto.From(brand);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var brand = await FindAsync(id, ct);
        brand.Deactivate(Now());
        await db.SaveChangesAsync(ct);
    }

    private async Task<Brand> FindAsync(int id, CancellationToken ct) =>
        await db.Brands.FirstOrDefaultAsync(b => b.Id == id, ct)
        ?? throw new NotFoundException("Brand", id);

    private async Task EnsureNameIsFreeAsync(string name, int? exceptId, CancellationToken ct)
    {
        // The default SQL Server collation is case-insensitive, so "acteck" clashes with "ACTECK".
        if (await db.Brands.AnyAsync(b => b.Name == name && b.Id != exceptId, ct))
        {
            throw new ConflictException($"A brand named '{name}' already exists.");
        }
    }

    private DateTime Now() => clock.GetUtcNow().UtcDateTime;
}
