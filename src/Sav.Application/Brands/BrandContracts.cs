using Sav.Domain.Brands;

namespace Sav.Application.Brands;

public sealed record SaveBrandRequest(string Name);

public sealed record BrandDto(int Id, string Name, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt)
{
    public static BrandDto From(Brand b) => new(b.Id, b.Name, b.IsActive, b.CreatedAt, b.UpdatedAt);
}
