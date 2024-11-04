using Microsoft.AspNetCore.SignalR;

namespace BlockChainAI
{
    public class PriceUpdateHub:Hub
    {
        public async Task SendPriceUpdate(string symbol, decimal price)
        {
            await Clients.All.SendAsync("ReceivePriceUpdate", symbol, price);
        }
    }
}
