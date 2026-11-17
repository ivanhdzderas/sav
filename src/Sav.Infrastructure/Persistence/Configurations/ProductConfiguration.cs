using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sav.Domain.Products;

namespace Sav.Infrastructure.Persistence.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code).HasMaxLength(Product.CodeMaxLength).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
        builder.Property(p => p.Description).HasMaxLength(Product.DescriptionMaxLength).IsRequired();
        builder.Property(p => p.Unit).HasMaxLength(Product.UnitMaxLength).IsRequired();
        builder.Property(p => p.Cost).HasPrecision(18, 2);
        builder.Property(p => p.Currency).HasMaxLength(3).IsFixedLength().IsRequired();

        builder.OwnsMany(p => p.Prices, prices =>
        {
            prices.ToTable("ProductPrices");
            prices.WithOwner().HasForeignKey("ProductId");
            prices.HasKey("ProductId", nameof(ProductPrice.Level));
            prices.Property(x => x.Level).ValueGeneratedNever();
            prices.Property(x => x.Amount).HasPrecision(18, 2);
        });
        builder.Navigation(p => p.Prices).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
