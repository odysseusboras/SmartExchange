using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using SmartExchange.Extensions;
using SmartExchange.Model.Enum;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SmartExchange.Model.ORM
{
    [Table("ToAsset")]
    public class ToAsset : BaseEntity
    {
        public required string Name { get; set; }
        public decimal TradePrice { get; set; } = 0;
        public decimal CurrentPrice { get; set; } = 0;
        public decimal PreviousPrice { get; set; } = 0;
        public decimal TradeQuantity { get; set; } = 0;
        public decimal TargetQuantity { get; set; } = 0;
        public decimal CurrentQuantity { get; set; } = 0;
        public decimal CurrentQuantityWithFee { get; set; } = 0;
        public decimal ProfitQuantityUSDT { get; set; } = 0;
        public decimal TotalQuantityUSDT { get; set; } = 0;
        public TransactionAction Action { get; set; }
        public bool Selected { get; set; } = false;
        public decimal PricePercentageDiff { get; set; } = 0;
        public bool Worth { get; set; } = false;
        public Guid FromAssetId { get; set; }
        [JsonIgnore]
        [ForeignKey("FromAssetId")]
        public FromAsset? FromAsset { get; set; }
        public int RoundsCheck { get; set; } = 0;
        public decimal StepSize { get; set; } = 0;
        public decimal QuantityPercentageDiff { get; set; } = 0;
        public decimal ThresholdBuy { get; set; } = 0;
        public decimal ThresholdSell { get; set; } = 0;
        [NotMapped]
        public decimal TargetPrice
        {
            get
            {
                decimal quantity = FromAsset?.Quantity ?? 0;
                return quantity == 0 || TargetQuantity == 0 ? 0 : Action == TransactionAction.Sell ? TargetQuantity / quantity : quantity / TargetQuantity;
            }
            set { }
        }

        [NotMapped]
        public string ActionName { get => Action.ToString(); set { } }
        [NotMapped]
        public bool PriceUp => CurrentPrice < TradePrice;
        [NotMapped]
        public bool? ActionPossibility => !(Action == TransactionAction.Buy && TradePrice >= CurrentPrice) &&
                    !(Action == TransactionAction.Sell && TradePrice <= CurrentPrice)
                    ? false
                    : (Action == TransactionAction.Buy && TradePrice > CurrentPrice) ||
                    (Action == TransactionAction.Sell && TradePrice < CurrentPrice)
        ? true
        : null;


        public void SetValues(decimal price, decimal USDTprice, Dictionary<string, decimal> maxQuantities, decimal thresholdSell, decimal thresholdBuy)
        {
            PreviousPrice = CurrentPrice;
            CurrentPrice = price;
            TradePrice = (TradePrice == 0) ? CurrentPrice : TradePrice;

            _ = maxQuantities.TryGetValue(Name, out decimal maxQuantity);

            if (Action == TransactionAction.Buy)
            {
                TradeQuantity = MathExtensions.Round((FromAsset?.Quantity ?? 0) / CurrentPrice, StepSize);
                CurrentQuantity = TradeQuantity;
            }
            else
            {
                TradeQuantity = MathExtensions.Round(FromAsset?.Quantity ?? 0, StepSize);
                CurrentQuantity = CurrentPrice * TradeQuantity;
            }
            CurrentQuantityWithFee = CurrentQuantity - (CurrentQuantity * 0.001M);
            TargetQuantity = (TargetQuantity != 0) ? TargetQuantity : Math.Max(CurrentQuantity, maxQuantity);
            PricePercentageDiff = Math.Abs(CurrentPrice - TradePrice) / TradePrice;
            ProfitQuantityUSDT = (TradeQuantity == 0) ? 0 : USDTprice * (CurrentQuantityWithFee - TargetQuantity);
            TotalQuantityUSDT = (CurrentQuantity == 0) ? 0 : USDTprice * CurrentQuantity;
            QuantityPercentageDiff = (CurrentQuantity == 0) ? 0 : (CurrentQuantityWithFee - TargetQuantity) / TargetQuantity;
            ThresholdBuy = thresholdBuy;
            ThresholdSell = thresholdSell;

        }
        public ToAsset Clone()
        {
            return new ToAsset()
            {
                Name = Name,
                TradePrice = TradePrice,
                CurrentPrice = CurrentPrice,
                TradeQuantity = TradeQuantity,
                Action = Action,
                Selected = Selected,
                PricePercentageDiff = PricePercentageDiff,
                Worth = Worth,
                RoundsCheck = RoundsCheck,
                PreviousPrice = PreviousPrice,
                StepSize = StepSize,
                TargetQuantity = TargetQuantity,
                CurrentQuantityWithFee = CurrentQuantityWithFee,
                CurrentQuantity = CurrentQuantity,
                FromAsset = FromAsset,
                ProfitQuantityUSDT = ProfitQuantityUSDT,
                TotalQuantityUSDT = TotalQuantityUSDT,
                QuantityPercentageDiff = QuantityPercentageDiff,
                ThresholdSell = ThresholdSell,
                ThresholdBuy = ThresholdBuy,
            };
        }
    }
    internal class ToAssetConfiguration : BaseEntityConfiguration<ToAsset>
    {
        public override void Configure(EntityTypeBuilder<ToAsset> builder)
        {
            _ = builder.HasIndex(c => c.Name);

            _ = builder.Property(e => e.TradePrice)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.CurrentPrice)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.PreviousPrice)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.TradeQuantity)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.PricePercentageDiff)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.ProfitQuantityUSDT)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.TotalQuantityUSDT)
                .HasPrecision(20, 8);

            _ = builder.Property(t => t.Action).HasConversion(new EnumToStringConverter<TransactionAction>());

            _ = builder.Property(e => e.StepSize)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.TargetQuantity)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.CurrentQuantity)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.CurrentQuantityWithFee)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.QuantityPercentageDiff)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.ThresholdBuy)
                .HasPrecision(20, 8);

            _ = builder.Property(e => e.ThresholdSell)
                .HasPrecision(20, 8);

            _ = builder.HasOne(er => er.FromAsset)
              .WithMany(e => e.ToAssets)
              .HasForeignKey(er => er.FromAssetId)
              .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

            base.Configure(builder);

        }
    }
}
