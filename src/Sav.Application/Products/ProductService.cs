using Microsoft.EntityFrameworkCore;
using Sav.Application.Common;
using Sav.Domain.Products;

namespace Sav.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct);

    Task<ProductDto> GetAsync(int id, CancellationToken ct);

    Task<ProductDto> CreateAsync(SaveProductRequest request, CancellationToken ct);

    Task<ProductDto> UpdateAsync(int id, SaveProductRequest request, CancellationToken ct);

    Task DeactivateAsync(int id, CancellationToken ct);
}

public sealed class ProductService(IApplicationDbContext db, TimeProvider clock) : IProductService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<ProductDto>> SearchAsync(string? search, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, MaxPageSize);

        var query = db.Products.AsNoTracking().Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.Code.Contains(term) || p.Description.Contains(term));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Description)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items.Select(ProductDto.From).ToList(), page, pageSize, total);
    }

    public async Task<ProductDto> GetAsync(int id, CancellationToken ct) =>
        ProductDto.From(await FindAsync(id, ct));

    public async Task<ProductDto> CreateAsync(SaveProductRequest request, CancellationToken ct)
    {
        var product = new Product(
            request.Code, request.Description, request.Unit, request.Cost, request.Currency, request.RequiresSerial, Now());
        product.SetPrices(ToTuples(request.Prices));

        await EnsureCodeIsFreeAsync(product.Code, exceptId: null, ct);

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);
        return ProductDto.From(product);
    }

    public async Task<ProductDto> UpdateAsync(int id, SaveProductRequest request, CancellationToken ct)
    {
        var product = await FindAsync(id, ct);
        product.Update(
            request.Code, request.Description, request.Unit, request.Cost, request.Currency, request.RequiresSerial, Now());
        product.SetPrices(ToTuples(request.Prices));

        await EnsureCodeIsFreeAsync(product.Code, exceptId: id, ct);

        await db.SaveChangesAsync(ct);
        return ProductDto.From(product);
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var product = await FindAsync(id, ct);
        product.Deactivate(Now());
        await db.SaveChangesAsync(ct);
    }

    private async Task<Product> FindAsync(int id, CancellationToken ct) =>
        await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
        ?? throw new NotFoundException("Product", id);

    private async Task EnsureCodeIsFreeAsync(string code, int? exceptId, CancellationToken ct)
    {
        // Deactivated products keep their code, so they are included in the check.
        if (await db.Products.AnyAsync(p => p.Code == code && p.Id != exceptId, ct))
        {
            throw new ConflictException($"A product with code '{code}' already exists.");
        }
    }

    private DateTime Now() => clock.GetUtcNow().UtcDateTime;

    private static IEnumerable<(int, decimal)> ToTuples(IReadOnlyList<PriceInput>? prices) =>
        (prices ?? []).Select(p => (p.Level, p.Amount));
}
