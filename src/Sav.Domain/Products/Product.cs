using Sav.Domain.Common;

namespace Sav.Domain.Products;

/// <summary>
/// A finished good or service that can be bought, stocked and sold.
/// Modernized version of the TBASERVICIOS table from the 2016 system.
/// </summary>
public sealed class Product
{
    public const int CodeMaxLength = 50;
    public const int DescriptionMaxLength = 150;
    public const int UnitMaxLength = 10;
    public const int MaxPriceLevels = 5;

    public static readonly IReadOnlySet<string> SupportedCurrencies = new HashSet<string> { "MXN", "USD" };

    private readonly List<ProductPrice> _prices = [];

    private Product() { } // EF Core

    public Product(string code, string description, string unit, decimal cost, string currency, bool requiresSerial, DateTime createdAt)
    {
        Apply(code, description, unit, cost, currency, requiresSerial);
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public string Unit { get; private set; } = string.Empty;

    public decimal Cost { get; private set; }

    public string Currency { get; private set; } = "MXN";

    public bool RequiresSerial { get; private set; }

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyCollection<ProductPrice> Prices => _prices.AsReadOnly();

    public void Update(string code, string description, string unit, decimal cost, string currency, bool requiresSerial, DateTime updatedAt)
    {
        Apply(code, description, unit, cost, currency, requiresSerial);
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Replaces the price list. Levels go from 1 to <see cref="MaxPriceLevels"/> and each one can appear once.
    /// Existing levels are updated in place so EF Core tracks them as modifications, not delete + insert.
    /// </summary>
    public void SetPrices(IEnumerable<(int Level, decimal Amount)> prices)
    {
        var incoming = prices.ToList();
        foreach (var (level, amount) in incoming)
        {
            if (level is < 1 or > MaxPriceLevels)
            {
                throw new DomainException($"Price level must be between 1 and {MaxPriceLevels}.");
            }

            if (amount < 0)
            {
                throw new DomainException("Price cannot be negative.");
            }
        }

        if (incoming.Select(p => p.Level).Distinct().Count() != incoming.Count)
        {
            throw new DomainException("Each price level can appear only once.");
        }

        _prices.RemoveAll(existing => incoming.All(p => p.Level != existing.Level));
        foreach (var (level, amount) in incoming)
        {
            var existing = _prices.Find(p => p.Level == level);
            if (existing is null)
            {
                _prices.Add(new ProductPrice(level, amount));
            }
            else
            {
                existing.Amount = amount;
            }
        }
    }

    /// <summary>Profit over cost for a price, as a percentage. Derived instead of stored (old UTILIDAD1..5 columns).</summary>
    public decimal? ProfitPercent(decimal price) =>
        Cost > 0 ? Math.Round((price - Cost) / Cost * 100, 2) : null;

    public void Deactivate(DateTime updatedAt)
    {
        IsActive = false;
        UpdatedAt = updatedAt;
    }

    private void Apply(string code, string description, string unit, decimal cost, string currency, bool requiresSerial)
    {
        if (cost < 0)
        {
            throw new DomainException("Cost cannot be negative.");
        }

        var normalizedCurrency = (currency ?? string.Empty).Trim().ToUpperInvariant();
        if (!SupportedCurrencies.Contains(normalizedCurrency))
        {
            throw new DomainException($"Currency must be one of: {string.Join(", ", SupportedCurrencies)}.");
        }

        Code = Required(code, nameof(Code), CodeMaxLength);
        Description = Required(description, nameof(Description), DescriptionMaxLength);
        Unit = Required(unit, nameof(Unit), UnitMaxLength);
        Cost = cost;
        Currency = normalizedCurrency;
        RequiresSerial = requiresSerial;
    }

    private static string Required(string? value, string field, int maxLength)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new DomainException($"{field} is required.");
        }

        if (trimmed.Length > maxLength)
        {
            throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        }

        return trimmed;
    }
}
