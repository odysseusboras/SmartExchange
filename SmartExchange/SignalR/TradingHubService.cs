using Microsoft.AspNetCore.SignalR;
using SmartExchange.Database;
using SmartExchange.Model.DTO;

namespace SmartExchange.SignalR
{
    public class TradingHubService
    {
        private readonly IHubContext<TradingHub> _hubContext;
        private readonly ExchangeDBContext _dbContext;
        public TradingHubService(ExchangeDBContext dbContext, IHubContext<TradingHub> hubContext)
        {
            _hubContext = hubContext;
            _dbContext = dbContext;
        }

        public async Task NotifyClients()
        {
            Overview overview = await _dbContext.GetOverview();

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", overview);
        }


    }

}
