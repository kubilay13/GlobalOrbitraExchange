using GlobalOrbitra.Db;
using GlobalOrbitra.Models.DTO.UserDto;
using GlobalOrbitra.Services.WalletService.WalletListenerService;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GlobalOrbitra.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly TronWalletListenerService _tronWalletListenerService;
        private readonly Dictionary<string, string> _coinLogos = new()
        {
            ["TRX"] = "https://cryptologos.cc/logos/tron-trx-logo.png",
            ["USDT"] = "https://cryptologos.cc/logos/tether-usdt-logo.png",
            ["BTC"] = "https://cryptologos.cc/logos/bitcoin-btc-logo.png",
            ["ETH"] = "https://cryptologos.cc/logos/ethereum-eth-logo.png"
        };
        public DashboardController(AppDbContext appDbContext,TronWalletListenerService tronWalletListenerService)
        {
            _tronWalletListenerService = tronWalletListenerService;
            _appDbContext = appDbContext;
        }
        private string GetLogoUrl(string symbol)
        {
            return _coinLogos.ContainsKey(symbol) ? _coinLogos[symbol] : "https://cryptologos.cc/logos/default.svg";
        }
        public async Task<IActionResult> Dashboard()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var userWallets = _appDbContext.UserWalletModels.Where(w => w.UserId == userId).ToList();
            var vmList = new List<WalletViewModel>();

            foreach (var wallet in userWallets)
            {
                // Sadece TRON cüzdanlarını işle
                if (!wallet.Network.Equals("TRON", StringComparison.OrdinalIgnoreCase))
                    continue;

                var balances = await _tronWalletListenerService.GetTronBalancesAsync(wallet.Address);

                if (balances.Count == 0)
                {
                    Console.WriteLine($"[Dashboard] {wallet.Address} için bakiye alınamadı.");
                    continue;
                }

                foreach (var kv in balances)
                {
                    Console.WriteLine($"[Dashboard] Token: {kv.Key}, Balance: {kv.Value}");

                    if (kv.Value <= 0)
                    {
                        Console.WriteLine($"[Dashboard] {kv.Key} bakiyesi 0 veya çok küçük, eklenmedi.");
                        continue;
                    }

                    vmList.Add(new WalletViewModel
                    {
                        Network = wallet.Network,
                        TokenSymbol = kv.Key,
                        Balance = kv.Value,
                        LogoUrl = GetLogoUrl(kv.Key) // Logo ekle
                        //price eklenecek
                    });
                }
            }

            return View(vmList);
        }
        public IActionResult TransactionHistory()
        {
            return View();
        }
    }
}
