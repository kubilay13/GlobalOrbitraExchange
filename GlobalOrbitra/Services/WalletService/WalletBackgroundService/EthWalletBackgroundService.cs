using GlobalOrbitra.Db;
using GlobalOrbitra.Services.WalletService.WalletListenerService;

namespace GlobalOrbitra.Services.WalletService.WalletBackgroundService
{
    public class EthWalletBackgroundService:BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EthWalletBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listener = new EthWalletListenerService();

            while (!stoppingToken.IsCancellationRequested)
            {
                await listener.GetEthWalletBalances("0x95c7352A085d962321Ee616E9E0632849b73717C");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
