using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartExchange.Model.Enum;
using SmartExchange.TradingProviders.Model;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartExchange.Model.ORM
{
    [Table("FromAsset")]
    public class FromAsset : BaseEntity
    {
        public string? Name { get; set; }
        public decimal Quantity { get; set; } = 0;
        public decimal TotalQuantityUSDT { get; set; } = 0;
        public List<ToAsset> ToAssets { get; set; } = [];
        public ToAsset GetToAsset(PairInfo pair)
        {
            string toCoinName = string.Equals(pair.FromCoinName, Name, StringComparison.Ordinal) ? pair.ToCoinName : pair.FromCoinName;

            ToAsset? item = ToAssets.FirstOrDefault(x => string.Equals(x.Name, toCoinName, StringComparison.Ordinal));

            if (item is null)
            {

                TransactionAction action = pair.FromCoinName == Name ? TransactionAction.Sell : TransactionAction.Buy;

                item = new()
                {
                    Name = toCoinName,
                    Action = action,
                    StepSize = pair.StepSize,
                    FromAsset = this
                };
                ToAssets.Add(item);
            }

            return item;
        }

        public FromAsset Clone()
        {
            FromAsset cloned = new()
            {
                Name = Name,
                Quantity = Quantity,
                TotalQuantityUSDT = TotalQuantityUSDT,
            };

            foreach (ToAsset toAsset in ToAssets)
            {
                ToAsset toAssetCloned = toAsset.Clone();
                toAssetCloned.FromAsset = cloned;
                cloned.ToAssets.Add(toAssetCloned);
            }
            return cloned;
        }
    }


    internal class FromAssetConfiguration : BaseEntityConfiguration<FromAsset>
    {
        public override void Configure(EntityTypeBuilder<FromAsset> builder)
        {
            _ = builder.HasIndex(c => c.Name);

            _ = builder.Property(e => e.Quantity)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.TotalQuantityUSDT)
                .HasPrecision(20, 8);

            base.Configure(builder);

        }
    }
}
