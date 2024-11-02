using SmartExchange.Model.ORM;

namespace SmartExchange.Model.DTO
{
    public class AssetProfit
    {
        public decimal Profit { get; set; }
    }
    public class AssetOverviewHistoryItem
    {
        public decimal Quantity { get; set; } = 0;
        public DateTime DateCreated { get; set; }
    }
    public class AssetOverviewHistory
    {
        public string? Name { get; set; }
        public List<AssetOverviewHistoryItem> Items { get; set; } = [];
        public AssetProfit assetProfit { get; set; } = new AssetProfit();
    }
    public class Overview
    {
        public decimal ThresholdBuy { get; set; }
        public decimal ThresholdSell { get; set; }
        public int TotalRoundsCheck { get; set; } = 0;
        public FromAsset? FromAsset { get; set; }
        public List<Transaction> transactions { get; set; } = [];
        public List<AssetOverviewHistory> historyAssets { get; set; } = [];

    }
}
