using GlobalOrbitra.Db;
using GlobalOrbitra.Services.WalletService.WalletListenerService;
using Microsoft.EntityFrameworkCore;

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
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var listener = new EthWalletListenerService(dbContext); // Sepolia sabit URL zaten listener içinde

                    var wallets = await dbContext.UserWalletModels.ToListAsync();

                    foreach (var wallet in wallets)
                    {
                        try
                        {
                            await listener.CheckIncomingForUserAsync(wallet);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Eth Background] Hata: {ex.Message}");
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken); // 10 saniyede bir kontrol
            }
        }
    }
}
