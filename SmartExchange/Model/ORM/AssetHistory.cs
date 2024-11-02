using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartExchange.Model.ORM
{
    [Table("AssetHistory")]
    public class AssetHistory : BaseEntity
    {
        public ICollection<AssetHistoryItem> AssetHistoryItems { get; set; } = [];
    }

    internal class AssetHistoryConfiguration : BaseEntityConfiguration<AssetHistory>
    {
        public override void Configure(EntityTypeBuilder<AssetHistory> builder)
        {
            base.Configure(builder);
        }
    }
}
