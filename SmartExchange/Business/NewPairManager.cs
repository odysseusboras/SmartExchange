using Microsoft.EntityFrameworkCore;
using SmartExchange.Database;
using SmartExchange.Extensions;
using SmartExchange.Model.Configuration;
using SmartExchange.Model.ORM;
using SmartExchange.SignalR;
using SmartExchange.TradingProviders;
using SmartExchange.TradingProviders.Model;

namespace SmartExchange.Business
{
    public class NewPairManager
    {
        private readonly ExchangeDBContext _dbContext;
        private readonly ITradingProvider _tradingProvider;

        private NewAsset? newAsset;
        public NewPairManager(ExchangeDBContext dbContext, ITradingProvider tradingProvider, AppSettings settings, TradingHubService tradingHubService)
        {
            _dbContext = dbContext;
            _tradingProvider = tradingProvider;
            //_settings = settings;
            //_tradingHubService = tradingHubService;
        }
        public async Task Initialize()
        {

            List<Pair> allBinancePairs = await GetAllBinancePairsAsync();

            List<Pair> newPairs = GetAllNew(allBinancePairs);

            _dbContext.Pairs.AddRange(newPairs);

            _ = await _dbContext.SaveChangesAsync();
        }
        public async Task RunAsync()
        {
            AccountInfo accountInfoDTO = await _tradingProvider.GetAccountInfo();

            newAsset = await _dbContext.NewAsset
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.Index)
                .FirstOrDefaultAsync();


            if (newAsset is null)
            {
                Asset USDTASSET = accountInfoDTO.Assets.Where(x => x.Name == "USDT").First();

                if (USDTASSET.Quantity >= 0)
                {
                    return;
                }

                List<Pair> allBinancePairs = await GetAllBinancePairsAsync();

                List<Pair> newPairs = GetAllNew(allBinancePairs);


                if (newPairs.Count > 0)
                {
                    Pair newPair = newPairs[0];

                    decimal price = await _tradingProvider.GetPairPriceAsync($"{newPair.Name}USDT");

                    decimal tradeQuantity = MathExtensions.Round(USDTASSET.Quantity / price, newPair.StepSize);

                    await _tradingProvider.BuyCoin($"{newPair.Name}USDT", tradeQuantity);

                    Thread.Sleep(100);

                    accountInfoDTO = await _tradingProvider.GetAccountInfo();

                    Asset asset = accountInfoDTO.Assets.First(x => x.Name == newPair.Name);

                    newAsset = new NewAsset()
                    {
                        Name = newPair.Name,
                        Quantity = asset.Quantity,
                        USDTQuantityBuy = tradeQuantity
                    };

                    _ = await _dbContext.NewAsset.AddAsync(newAsset);

                    _ = await _dbContext.SaveChangesAsync();
                }
            }
            else
            {

                Pair pair = _dbContext.Pairs.First(x => x.Name == $"{newAsset.Name}USDT");

                decimal price = await _tradingProvider.GetPairPriceAsync($"{newAsset.Name}USDT");

                decimal tradeQuantity = MathExtensions.Round(newAsset?.Quantity ?? 0, pair.StepSize);

                decimal newUSDTQuantity = price * tradeQuantity;

                decimal percentageChange = (newUSDTQuantity - newAsset.USDTQuantityBuy) / newAsset.USDTQuantityBuy;

                if (percentageChange > 0.05M)
                {
                    await _tradingProvider.SellCoin($"{newAsset.Name}USDT", tradeQuantity);

                    newAsset.IsActive = false;

                    newAsset.USDTQuantitySell = tradeQuantity;

                    _ = await _dbContext.SaveChangesAsync();

                    newAsset = null;

                }
            }
        }

        #region Helpers

        private async Task<List<Pair>> GetAllBinancePairsAsync()
        {
            return (await _tradingProvider.GetAllPairs())
                .Select(pairInfo => new Pair
                {
                    Name = pairInfo.Name,
                    StepSize = pairInfo.StepSize
                })
                .ToList();
        }

        private List<Pair> GetAllNew(List<Pair> pairs)
        {
            IQueryable<Pair> existingPairs = _dbContext.Pairs.AsNoTracking();

            return pairs
                .Except(existingPairs, new PairComparer())
                .ToList();
        }


        #endregion
    }
}
