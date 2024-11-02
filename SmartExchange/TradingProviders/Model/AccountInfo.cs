namespace SmartExchange.TradingProviders.Model
{
    public class Asset
    {
        public required string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityUSDT { get; set; }
    }
    public class AccountInfo
    {
        public List<Asset> Assets = [];
    }
}
