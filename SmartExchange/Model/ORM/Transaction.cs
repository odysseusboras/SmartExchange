using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartExchange.Model.Enum;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartExchange.Model.ORM
{
    [Table("Transaction")]
    public class Transaction : BaseEntity
    {
        public decimal? Quantity { get; set; }
        public TransactionAction Action { get; set; }
        [NotMapped]
        public string ActionName { get => Action.ToString(); set { } }
        public string? FromAssetName { get; set; }
        public string? ToAssetName { get; set; }
        public decimal? Price { get; set; }
        public decimal? PreviousPrice { get; set; }

    }
    internal class TransactionConfiguration : BaseEntityConfiguration<Transaction>
    {
        public override void Configure(EntityTypeBuilder<Transaction> builder)
        {
            _ = builder.Property(e => e.Quantity)
                .HasPrecision(20, 8);

            _ = builder.Property(t => t.Action).HasConversion(new EnumToStringConverter<TransactionAction>());

            _ = builder.Property(e => e.Price)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.PreviousPrice)
                .HasPrecision(20, 8);

            base.Configure(builder);
        }
    }
}
