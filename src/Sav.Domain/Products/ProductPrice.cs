namespace Sav.Domain.Products;

/// <summary>
/// One sale price level of a product. Replaces the fixed PRECIO1..PRECIO5 columns
/// of the original TBASERVICIOS table.
/// </summary>
public sealed class ProductPrice
{
    private ProductPrice() { } // EF Core

    internal ProductPrice(int level, decimal amount)
    {
        Level = level;
        Amount = amount;
    }

    public int Level { get; private set; }

    public decimal Amount { get; internal set; }
}
