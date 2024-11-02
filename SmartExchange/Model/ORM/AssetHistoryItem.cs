using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartExchange.Model.ORM
{
    [Table("AssetHistoryItem")]
    public class AssetHistoryItem : BaseEntity
    {
        public required string Name { get; set; }
        public decimal Quantity { get; set; } = 0;
        public Guid AssetHistoryId { get; set; }
        [JsonIgnore]
        [ForeignKey("AssetHistoryId")]
        public AssetHistory AssetHistory { get; set; } = new AssetHistory();
    }
    internal class AssetHistoryItemConfiguration : BaseEntityConfiguration<AssetHistoryItem>
    {
        public override void Configure(EntityTypeBuilder<AssetHistoryItem> builder)
        {
            _ = builder.Property(e => e.Quantity)
                .HasPrecision(20, 8);

            _ = builder.HasOne(er => er.AssetHistory)
              .WithMany(e => e.AssetHistoryItems)
              .HasForeignKey(er => er.AssetHistoryId);

            base.Configure(builder);

        }
    }
}
