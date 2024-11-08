﻿using SmartExchange.Model.Configuration;
using SmartExchange.TradingProviders.Model;

namespace SmartExchange.TradingProviders
{
    public interface ITradingProvider
    {
        Task<AccountInfo> GetAccountInfo(List<ConfigPair>? configPairs = null);
        Task<IEnumerable<PairInfo>> GetAllPairs(List<ConfigPair>? configPairs = null);
        Task<decimal> GetPairPriceAsync(string pairName);
        Task SellCoin(string pairName, decimal amount);
        Task BuyCoin(string pairName, decimal amount);

    }
}
