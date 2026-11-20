using Sav.Domain.Common;

namespace Sav.Domain.Brands;

/// <summary>
/// Manufacturer or label of a product (Transcend, Acteck...).
/// Modernized version of the TBAMARCA table from the 2016 system.
/// </summary>
public sealed class Brand
{
    public const int NameMaxLength = 50;

    private Brand() { } // EF Core

    public Brand(string name, DateTime createdAt)
    {
        Name = ValidName(name);
        CreatedAt = createdAt;
    }

    public int Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public bool IsActive { get; private set; } = true;

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public void Rename(string name, DateTime updatedAt)
    {
        Name = ValidName(name);
        UpdatedAt = updatedAt;
    }

    /// <summary>Soft delete: products already linked to the brand keep pointing to it.</summary>
    public void Deactivate(DateTime updatedAt)
    {
        IsActive = false;
        UpdatedAt = updatedAt;
    }

    private static string ValidName(string? name)
    {
        var trimmed = name?.Trim() ?? string.Empty;
        if (trimmed.Length == 0)
        {
            throw new DomainException("Name is required.");
        }

        if (trimmed.Length > NameMaxLength)
        {
            throw new DomainException($"Name cannot exceed {NameMaxLength} characters.");
        }

        return trimmed;
    }
}
