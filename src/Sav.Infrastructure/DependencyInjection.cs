using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Sav.Application.Common;
using Sav.Infrastructure.Persistence;

namespace Sav.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'Sav' is not configured.");
        }

        services.AddDbContext<SavDbContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<SavDbContext>());
        return services;
    }
}
