namespace SmartExchange.Model.Configuration
{

    public class TradingProvider
    {
        public required string Name { get; set; }
        public required string BaseUrl { get; set; }
        public required string ApiKey { get; set; }
        public required string Secret { get; set; }
    }
    public class ConfigPair
    {
        public required string Name { get; set; }
        public decimal ThresholdBuy { get; set; } = 0.005m;
        public decimal ThresholdSell { get; set; } = 0.002m;
    }
    public class AppSettings
    {
        public TradingProvider? TradingProvider { get; set; }
        public int Interval { get; set; }
        public int RoundsCheck { get; set; } = 3;
        public List<ConfigPair> Assets { get; set; } = [];
        public bool StopOnError { get; set; } = false;
        public bool DebugMode { get; set; } = false;

    }
}
