using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SmartExchange.Business;
using SmartExchange.Database;
using SmartExchange.Model.Configuration;
using SmartExchange.SignalR;
using SmartExchange.TradingProviders;

namespace SmartExchange
{
    public class ScheduledJobService : BackgroundService
    {
        private ExchangeManager? _exchangeManager;
        private readonly AppSettings _settings;
        private ITradingProvider? _tradingProvider;
        private ExchangeDBContext? _dbContext;
        private TradingHubService? _tradingHubService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public static CancellationToken CancellationToken { get; private set; }
        public ScheduledJobService(IOptions<AppSettings> settings, IServiceScopeFactory serviceScopeFactory)
        {
            _settings = settings.Value;
            _serviceScopeFactory = serviceScopeFactory;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            CancellationToken = stoppingToken;
            using IServiceScope scope = _serviceScopeFactory.CreateScope();
            _tradingProvider = scope.ServiceProvider.GetRequiredService<ITradingProvider>();
            _dbContext = scope.ServiceProvider.GetRequiredService<ExchangeDBContext>();
            _tradingHubService = scope.ServiceProvider.GetRequiredService<TradingHubService>();

            _exchangeManager = new ExchangeManager(_dbContext, _tradingProvider, _settings, _tradingHubService);

            await _exchangeManager.Initialize();

            while (!CancellationToken.IsCancellationRequested)
            {
                try
                {
                    await _exchangeManager.RunAsync();
                }
                catch
                {
                    if (_settings.StopOnError)
                    {
                        throw;
                    }
                }
                finally
                {
                    await Task.Delay(_settings.Interval, CancellationToken);
                }
            }

        }
    }
}
