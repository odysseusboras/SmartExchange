using Microsoft.AspNetCore.SignalR;

namespace SmartExchange.SignalR
{
    public class TradingHub : Hub
    {
        public TradingHubService _tradingHubService { get; }
        public TradingHub(TradingHubService tradingHubService)
        {
            _tradingHubService = tradingHubService;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await _tradingHubService.NotifyClients();
        }
    }
}
