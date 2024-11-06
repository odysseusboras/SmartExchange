using SmartExchange.Database;
using SmartExchange.Extensions;
using SmartExchange.Model.Configuration;
using SmartExchange.Model.Enum;
using SmartExchange.Model.ORM;
using SmartExchange.SignalR;
using SmartExchange.TradingProviders;
using SmartExchange.TradingProviders.Model;

namespace SmartExchange.Business
{
    public class ExchangeManager
    {
        private readonly ExchangeDBContext _dbContext;
        private readonly ITradingProvider _tradingProvider;
        private readonly AppSettings _settings;
        private readonly TradingHubService _tradingHubService;
        private IEnumerable<PairInfo>? pairs;
        private FromAsset fromAsset { get; set; } = new FromAsset();

        public ExchangeManager(ExchangeDBContext dbContext, ITradingProvider tradingProvider, AppSettings settings, TradingHubService tradingHubService)
        {
            _dbContext = dbContext;
            _tradingProvider = tradingProvider;
            _settings = settings;
            _tradingHubService = tradingHubService;
        }

        public async Task Initialize()
        {
            pairs ??= await _tradingProvider.GetAllPairs(_settings.Pairs ?? []);

            fromAsset = await _dbContext.GetLatestStepAsync() ?? await GetStartingAsset() ?? throw new Exception("From asset cannot be empty");

            await _dbContext.LogCurrentStep(fromAsset);
        }

        public async Task RunAsync()
        {
            Dictionary<string, decimal> maxQuantities = _dbContext.AssetHistoryItems
                .GroupBy(item => item.Name)
                .Select(group => new
                {
                    Name = group.Key,
                    MaxQuantity = group.Max(item => item.Quantity)
                })
                .ToDictionary(result => result.Name, result => result.MaxQuantity);

            IEnumerable<PairInfo> pairsInfo = Helpers.GetRelevantPairs(pairs ?? [], fromAsset, _settings.Pairs);


            decimal USDTprice = (fromAsset.Name != "USDT") ? await _tradingProvider.GetPairPriceAsync($"{fromAsset.Name}USDT") : 1;

            fromAsset.TotalQuantityUSDT = USDTprice * fromAsset.Quantity;

            foreach (PairInfo pair in pairsInfo)
            {
                ToAsset toAsset = fromAsset.GetToAsset(pair);

                USDTprice = (toAsset.Name != "USDT") ? await _tradingProvider.GetPairPriceAsync($"{toAsset.Name}USDT") : 1;

                ConfigPair configPair = _settings.Pairs.First(x => x.Name == pair.Name);

                decimal currentPrice = await _tradingProvider.GetPairPriceAsync(pair.Name);

                toAsset.SetValues(currentPrice, USDTprice, maxQuantities, configPair.ThresholdSell, configPair.ThresholdBuy);

                if (WorthAction(toAsset))
                {
                    if (await ExecuteActionAsync(toAsset.Action, pair, toAsset))
                    {
                        break;
                    }
                }
            }
            await _dbContext.LogCurrentStep(fromAsset);
            await _tradingHubService.NotifyClients();
        }

        #region Helpers       
        private bool WorthAction(ToAsset toAsset)
        {
            bool percentageCheck = toAsset.QuantityPercentageDiff > (toAsset.Action == TransactionAction.Sell ? toAsset.ThresholdSell : toAsset.ThresholdBuy);

            if (toAsset.CurrentQuantityWithFee > toAsset.TargetQuantity && percentageCheck)
            {
                toAsset.RoundsCheck++;

                if (toAsset.RoundsCheck > _settings.RoundsCheck)
                {
                    toAsset.Selected = (toAsset.Action == TransactionAction.Buy)
                        ? toAsset.CurrentPrice > toAsset.PreviousPrice
                        : toAsset.CurrentPrice < toAsset.PreviousPrice;
                }

                toAsset.Worth = true;
            }
            else
            {
                toAsset.RoundsCheck = 0;
                toAsset.Selected = false;
                toAsset.Worth = false;
            }

            return toAsset.Selected;
        }
        private async Task<bool> ExecuteActionAsync(TransactionAction action, PairInfo pair, ToAsset toAsset)
        {
            try
            {
                if (!_settings.DebugMode)
                {
                    if (action == TransactionAction.Sell)
                    {
                        await _tradingProvider.SellCoin(pair.Name, toAsset.TradeQuantity);
                    }
                    else
                    {
                        await _tradingProvider.BuyCoin(pair.Name, toAsset.TradeQuantity);
                    }
                }
            }
            catch
            {
                toAsset.Selected = false;
                return false;
            }

            await PostActionAsync();

            return true;
        }
        private async Task PostActionAsync()
        {
            await _dbContext.LogTransaction(fromAsset);

            Thread.Sleep(100);

            fromAsset = await GetStartingAsset();

            await RunAsync();

            await LogAssetsAsync();

        }
        private async Task<FromAsset> GetStartingAsset()
        {
            AccountInfo accountInfoDTO = await _tradingProvider.GetAccountInfo(_settings.Pairs);

            foreach (Asset asset in accountInfoDTO.Assets)
            {
                if (asset.Name == "USDT")
                {
                    asset.QuantityUSDT = asset.Quantity;
                }
                else
                {
                    decimal pairPrice = await _tradingProvider.GetPairPriceAsync($"{asset.Name}USDT");

                    asset.QuantityUSDT = asset.Quantity * pairPrice;
                }
            }

            Asset startingCoin = accountInfoDTO.Assets.MaxBy(x => x.QuantityUSDT) ?? throw new Exception("Assets cannot be empty");

            fromAsset = new() { Name = startingCoin.Name, Quantity = startingCoin.Quantity };

            return fromAsset;
        }
        private async Task LogAssetsAsync()
        {
            AccountInfo accountInfoDTO = await _tradingProvider.GetAccountInfo(_settings.Pairs);

            await _dbContext.LogAccountAssets(accountInfoDTO);
        }


        #endregion
    }
}
