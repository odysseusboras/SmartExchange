using Microsoft.AspNetCore.SignalR;

namespace SmartExchange.SignalR
{
    public class TradingHub : Hub
    {
        public TradingHub()
        {
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

    }
}
