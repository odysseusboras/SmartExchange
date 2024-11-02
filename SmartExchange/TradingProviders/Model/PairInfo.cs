namespace SmartExchange.TradingProviders.Model
{
    public class PairInfo
    {
        public required string PairName { get; set; }
        public required string FromCoinName { get; set; }
        public required string ToCoinName { get; set; }
        public required decimal MinQuantity { get; set; }
        public required decimal MaxQuantity { get; set; }
        public required decimal StepSize { get; set; }
        public string Name => $"{FromCoinName}{ToCoinName}";
    }
}
