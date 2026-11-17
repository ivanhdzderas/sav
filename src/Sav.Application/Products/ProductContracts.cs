using Sav.Domain.Products;

namespace Sav.Application.Products;

public sealed record PriceInput(int Level, decimal Amount);

public sealed record SaveProductRequest(
    string Code,
    string Description,
    string Unit,
    decimal Cost,
    string Currency,
    bool RequiresSerial,
    IReadOnlyList<PriceInput>? Prices);

public sealed record ProductPriceDto(int Level, decimal Amount, decimal? ProfitPercent);

public sealed record ProductDto(
    int Id,
    string Code,
    string Description,
    string Unit,
    decimal Cost,
    string Currency,
    bool RequiresSerial,
    bool IsActive,
    IReadOnlyList<ProductPriceDto> Prices,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static ProductDto From(Product p) => new(
        p.Id,
        p.Code,
        p.Description,
        p.Unit,
        p.Cost,
        p.Currency,
        p.RequiresSerial,
        p.IsActive,
        p.Prices.OrderBy(x => x.Level).Select(x => new ProductPriceDto(x.Level, x.Amount, p.ProfitPercent(x.Amount))).ToList(),
        p.CreatedAt,
        p.UpdatedAt);
}
