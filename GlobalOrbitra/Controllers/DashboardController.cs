using GlobalOrbitra.Db;
using GlobalOrbitra.Models.DTO.UserDto;
using GlobalOrbitra.Services.WalletService.WalletListenerService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GlobalOrbitra.Controllers
{
    public class DashboardController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly TronWalletListenerService _tronWalletListenerService;

        public DashboardController(AppDbContext appDbContext,TronWalletListenerService tronWalletListenerService)
        {
            _tronWalletListenerService = tronWalletListenerService;
            _appDbContext = appDbContext;
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
                        //price eklenecek
                    });
                }
            }

            return View(vmList);
        }
        public async Task<IActionResult> TransactionHistory()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Kullanıcının cüzdan adreslerini al
            var userWallets = await _appDbContext.UserWalletModels
                .Where(w => w.UserId == userId)
                .Select(w => w.Address)
                .ToListAsync();

            // Bu cüzdanlarla ilişkili transaction'ları getir
            var transactions = await _appDbContext.AssetTransactionModels
                .Include(t => t.Token)
                .Where(t => t.UserId == userId || userWallets.Contains(t.WalletAddress) || userWallets.Contains(t.SenderAddress))
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TransactionViewModel
                {
                    Id = t.Id,
                    Date = t.CreatedAt,
                    Type = t.Type,
                    Asset = t.Token != null ? t.Token.Symbol : "UNKNOWN",
                    Amount = t.Amount,
                    Fee = t.Commission,
                    Status = t.Status,
                    TxHash = t.TxHash,
                    // FromTo: Deposit ise gönderen adres (dış), Withdrawal ise alıcı adres (dış)
                    FromTo = t.Type == "deposit" ? t.SenderAddress : t.WalletAddress,
                    WalletAddress = t.WalletAddress,
                    SenderAddress = t.SenderAddress
                })
                .ToListAsync();

            return View(transactions);
        }
    }
}
