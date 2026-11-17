using Microsoft.EntityFrameworkCore;
using Sav.Domain.Products;

namespace Sav.Application.Common;

/// <summary>Persistence boundary used by the use cases; implemented in Sav.Infrastructure.</summary>
public interface IApplicationDbContext
{
    DbSet<Product> Products { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
