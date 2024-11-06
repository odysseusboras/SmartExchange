using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartExchange.Model.ORM
{
    [Table("NewAsset")]
    public class NewAsset : BaseEntity
    {
        public string? Name { get; set; }
        public decimal USDTQuantityBuy { get; set; } = 0;
        public decimal USDTQuantitySell { get; set; } = 0;
        public decimal Quantity { get; set; } = 0;
        public bool IsActive { get; set; } = true;
    }


    internal class NewAssetConfiguration : BaseEntityConfiguration<NewAsset>
    {
        public override void Configure(EntityTypeBuilder<NewAsset> builder)
        {
            _ = builder.HasIndex(c => c.Name);

            _ = builder.Property(e => e.USDTQuantityBuy)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.USDTQuantitySell)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.Quantity)
                .HasPrecision(20, 8);


            base.Configure(builder);

        }
    }
}
