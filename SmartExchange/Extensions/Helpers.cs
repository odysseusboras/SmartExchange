using SmartExchange.Model.Configuration;
using SmartExchange.Model.ORM;
using SmartExchange.TradingProviders.Model;

namespace SmartExchange.Extensions
{
    public static class Helpers
    {
        public static IEnumerable<PairInfo> GetRelevantPairs(IEnumerable<PairInfo> pairs, FromAsset fromAsset, List<ConfigPair> assets)
        {
            return pairs.Where(x =>
                                assets.Any(y => y.Name.Contains(x.FromCoinName)) &&
                                assets.Any(y => y.Name.Contains(x.ToCoinName)) &&
                                (x.FromCoinName == fromAsset.Name || x.ToCoinName == fromAsset.Name)
                        )
                .OrderByDescending(x =>
                {
                    ToAsset? toAsset = fromAsset.ToAssets.FirstOrDefault(toAsset => toAsset.Name == x.FromCoinName || toAsset.Name == x.ToCoinName);
                    return toAsset?.profitQuantityUSDT ?? 0;
                });

        }

    }
}
