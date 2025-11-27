using GlobalOrbitra.Db;
using GlobalOrbitra.Services.WalletService.WalletListenerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace GlobalOrbitra.Services.WalletService.WalletBackgroundService
{
    public class EthWalletBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public EthWalletBackgroundService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var listener = new EthWalletListenerService(dbContext);

                    try
                    {
                        await listener.CheckAllEthWallets();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[BACKGROUND ERROR] {ex.Message}");
                    }
                }

                // 15 saniyede bir kontrol et
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}