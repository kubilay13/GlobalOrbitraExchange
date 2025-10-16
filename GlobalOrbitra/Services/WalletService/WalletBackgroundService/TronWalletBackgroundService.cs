using GlobalOrbitra.Db;
using GlobalOrbitra.Services.WalletService.WalletListenerService;
using Microsoft.EntityFrameworkCore;

namespace GlobalOrbitra.Services.WalletService.WalletBackgroundService
{
    public class TronWalletBackgroundService:BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public TronWalletBackgroundService(IServiceScopeFactory serviceScopeFactory)
        {
            _scopeFactory = serviceScopeFactory;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var tronService = new TronWalletListenerService(db);

                    try
                    {
                        var wallets = db.UserWalletModels.Include(w => w.Token).ThenInclude(t => t.Chain).Where(w => w.Token.Chain.Symbol.ToUpper() == "TRX").ToList();

                        foreach (var wallet in wallets)
                        {
                            await tronService.CheckIncomingForUserAsync(wallet);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TRON Listener] Hata: {ex.Message}");
                    }
                }

                // 10 saniye bekle, sonra tekrar çalışsın
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }
    }
}
