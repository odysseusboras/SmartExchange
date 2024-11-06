using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SmartExchange.Model.Configuration;
using SmartExchange.Model.DTO;
using SmartExchange.Model.ORM;
using SmartExchange.TradingProviders.Model;

namespace SmartExchange.Database
{
    public class ExchangeDBContext : DbContext, IDisposable
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<FromAsset> FromAssets { get; set; }
        public DbSet<ToAsset> ToAssets { get; set; }
        public DbSet<AssetHistory> AssetHistories { get; set; }
        public DbSet<AssetHistoryItem> AssetHistoryItems { get; set; }
        public DbSet<Pair> Pairs { get; set; }
        public DbSet<NewAsset> NewAsset { get; set; }


        private readonly AppSettings _settings;

        public ExchangeDBContext(DbContextOptions<ExchangeDBContext> options, IOptions<AppSettings> settings)
            : base(options)
        {
            _settings = settings.Value;

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ExchangeDBContext).Assembly);
        }

        public override int SaveChanges()
        {
            foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.DateCreated = DateTime.UtcNow;
                }
            }

            return base.SaveChanges();
        }

        public async Task<FromAsset?> GetLatestStepAsync()
        {
            return await FromAssets
                .Include(x => x.ToAssets)
                .OrderByDescending(x => x.Index)
                .AsNoTracking()
                .AsSplitQuery()
                .FirstOrDefaultAsync();
        }

        public async Task LogCurrentStep(FromAsset fromAsset)
        {

            FromAsset fromAssetCloned = fromAsset.Clone();

            _ = await FromAssets.AddAsync(fromAssetCloned);

            _ = await FromAssets
                  .Where(t => t.DateCreated <= DateTime.UtcNow.AddHours(-2))
                  .ExecuteDeleteAsync();

            _ = await SaveChangesAsync();

        }

        public async Task LogTransaction(FromAsset fromAsset)
        {
            ToAsset toAsset = fromAsset.ToAssets.First(x => x.Selected);
            Transaction transaction = new()
            {
                Quantity = toAsset?.TradeQuantity,
                Action = toAsset?.Action ?? throw new Exception("To asset cannot be empty"),
                FromAssetName = fromAsset?.Name,
                ToAssetName = toAsset?.Name,
                Price = toAsset?.CurrentPrice,
                PreviousPrice = toAsset?.TradePrice
            };
            _ = await Transactions.AddAsync(transaction);

            _ = await SaveChangesAsync();
        }

        public async Task LogAccountAssets(AccountInfo accountInfoDTO)
        {
            AssetHistory newHistory = new() { };
            foreach (Asset asset in accountInfoDTO.Assets)
            {
                newHistory.AssetHistoryItems.Add(new AssetHistoryItem()
                {
                    Name = asset.Name,
                    Quantity = asset.Quantity,
                    AssetHistory = newHistory
                });
            }

            _ = await AssetHistories.AddAsync(newHistory);

            _ = await SaveChangesAsync();
        }


        public async Task<Overview> GetOverview()
        {
            FromAsset? fromAsset = await GetLatestStepAsync();
            if (fromAsset is null)
            {
                return new Overview();
            }

            fromAsset.ToAssets = fromAsset.ToAssets
                .OrderByDescending(x => x.ActionPossibility)
                .ThenByDescending(x => x.ProfitQuantityUSDT)
                .ToList();

            List<Transaction> transactions = Transactions
                .AsNoTracking()
                .OrderByDescending(x => x.DateCreated).ToList();

            Overview overView = new()
            {
                FromAsset = fromAsset,
                transactions = transactions,
                TotalRoundsCheck = _settings.RoundsCheck
            };
            foreach (IGrouping<string, AssetHistoryItem> asset in AssetHistoryItems
                .AsNoTracking()
                .AsSplitQuery()
                .GroupBy(x => x.Name))
            {
                AssetOverviewHistory assetHistory = new()
                {
                    Name = asset.Key,
                    Items = asset
                        .OrderBy(x => x.DateCreated)
                        .Select(x => new AssetOverviewHistoryItem()
                        {
                            DateCreated = x.DateCreated.ToLocalTime(),
                            Quantity = x.Quantity
                        })
                        .ToList()
                };

                overView.historyAssets.Add(assetHistory);
            }


            return overView;
        }
    }
}
