using GlobalOrbitra.Db;
using GlobalOrbitra.Models.UserModel;
using Microsoft.EntityFrameworkCore;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;

namespace GlobalOrbitra.Services.WalletService.WalletListenerService
{
    public class EthWalletListenerService
    {
        private readonly AppDbContext _dbContext;
        private readonly Web3 _web3;
        private const string SepoliaRpcUrl = "https://sepolia.infura.io/v3/b564b65594c54416aa2cd00405060088";
        private long _lastProcessedBlock = 0;

        public EthWalletListenerService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _web3 = new Web3(SepoliaRpcUrl);
        }

        public async Task CheckAllEthWallets()
        {
            try
            {
                // Sadece ETH cüzdanlarını al
                var ethWallets = await _dbContext.UserWalletModels
                    .Where(w => w.Network == "ETH")
                    .ToListAsync();

                if (!ethWallets.Any())
                {
                    Console.WriteLine("[INFO] ETH cüzdanı bulunamadı");
                    return;
                }

                Console.WriteLine($"[INFO] {ethWallets.Count} ETH cüzdanı taranacak");

                // Son işlenen blok numarasını al
                if (_lastProcessedBlock == 0)
                {
                    _lastProcessedBlock = await GetLastProcessedBlock();
                    Console.WriteLine($"[INFO] Son işlenen blok: {_lastProcessedBlock}");
                }

                // Şu anki blok numarasını al
                var currentBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                var currentBlockNumber = (long)currentBlock.Value;

                Console.WriteLine($"[INFO] Şu anki blok: {currentBlockNumber}");

                if (_lastProcessedBlock >= currentBlockNumber)
                {
                    Console.WriteLine($"[INFO] Yeni blok yok. Son blok: {currentBlockNumber}");
                    return;
                }

                // Maksimum 100 blok tara (performans için)
                var startBlock = _lastProcessedBlock + 1;
                var endBlock = Math.Min(currentBlockNumber, _lastProcessedBlock + 100);

                Console.WriteLine($"[INFO] {startBlock} - {endBlock} blokları taranıyor (Toplam: {endBlock - startBlock + 1} blok)");

                int totalTransactionsFound = 0;

                // Blokları teker teker tara
                for (long blockNumber = startBlock; blockNumber <= endBlock; blockNumber++)
                {
                    var transactionsInBlock = await ProcessBlock(blockNumber, ethWallets);
                    totalTransactionsFound += transactionsInBlock;
                    _lastProcessedBlock = blockNumber;
                }

                Console.WriteLine($"[INFO] Tarama tamamlandı. Toplam {totalTransactionsFound} işlem bulundu.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] ETH cüzdan kontrol hatası: {ex.Message}");
            }
        }

        private async Task<long> GetLastProcessedBlock()
        {
            try
            {
                // Veritabanındaki en son işlemin blok numarasını al
                var lastTransaction = await _dbContext.Transactions
                    .Where(t => t.TokenId == 0) // ETH işlemleri
                    .OrderByDescending(t => t.CreatedAt)
                    .FirstOrDefaultAsync();

                var latestBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                var latestBlockNumber = (long)latestBlock.Value;

                if (lastTransaction != null)
                {
                    // Son 1000 bloğu tara
                    var startBlock = latestBlockNumber - 1000;
                    Console.WriteLine($"[INFO] Son işlem bulundu. {startBlock} bloğundan itibaren taranacak");
                    return Math.Max(0, startBlock);
                }

                // İlk çalışmada son 100 bloğu tara
                var firstBlock = latestBlockNumber - 100;
                Console.WriteLine($"[INFO] İlk çalışma. {firstBlock} bloğundan itibaren taranacak");
                return Math.Max(0, firstBlock);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Son blok alınırken hata: {ex.Message}");
                // Hata durumunda son 50 bloğu tara
                var latestBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();
                return Math.Max(0, (long)latestBlock.Value - 50);
            }
        }

        private async Task<int> ProcessBlock(long blockNumber, List<UserWalletModel> wallets)
        {
            try
            {
             //   Console.WriteLine($"[BLOCK {blockNumber}] Blok taranıyor...");

                var block = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber
                    .SendRequestAsync(new HexBigInteger(blockNumber));

                if (block?.Transactions == null)
                {
                    Console.WriteLine($"[BLOCK {blockNumber}] İşlem yok");
                    return 0;
                }

               // Console.WriteLine($"[BLOCK {blockNumber}] {block.Transactions.Length} işlem taranıyor");

                int foundTransactions = 0;

                foreach (var tx in block.Transactions)
                {
                    var found = await ProcessTransaction(tx, wallets, blockNumber);
                    if (found) foundTransactions++;
                }

                if (foundTransactions > 0)
                {
                    Console.WriteLine($"[BLOCK {blockNumber}] {foundTransactions} işlem bulundu");
                }

                return foundTransactions;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Blok {blockNumber} işlenirken hata: {ex.Message}");
                return 0;
            }
        }

        private async Task<bool> ProcessTransaction(Nethereum.RPC.Eth.DTOs.Transaction tx, List<UserWalletModel> wallets, long blockNumber)
        {
            try
            {
                // Geçersiz işlemleri atla
                if (tx.To == null || string.IsNullOrEmpty(tx.To))
                {
                    return false;
                }

                if (tx.Value == null || tx.Value.Value == 0)
                {
                    return false;
                }

                var toAddress = tx.To.Trim().ToLower();
                var amount = Web3.Convert.FromWei(tx.Value.Value);

                // Çok küçük miktarları da göster (0.00005 ETH gibi)
                // Console.WriteLine($"[TX {tx.TransactionHash}] {amount} ETH -> {toAddress}");

                // İlgili cüzdanı bul
                var wallet = wallets.FirstOrDefault(w =>
                    w.Address.Trim().ToLower() == toAddress);

                if (wallet == null)
                {
                    return false;
                }

                Console.WriteLine($"[MATCH FOUND] {amount} ETH {wallet.Address} cüzdanına geldi!");

                await SaveTransaction(wallet, tx, amount);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] İşlem {tx.TransactionHash} işlenirken hata: {ex.Message}");
                return false;
            }
        }

        private async Task SaveTransaction(UserWalletModel wallet, Nethereum.RPC.Eth.DTOs.Transaction tx, decimal amount)
        {
            try
            {
                // Aynı işlem daha önce kaydedilmiş mi kontrol et
                var existingTx = await _dbContext.Transactions
                    .FirstOrDefaultAsync(t => t.TxHash == tx.TransactionHash);

                if (existingTx != null)
                {
                    Console.WriteLine($"[SKIP] İşlem zaten kayıtlı: {tx.TransactionHash}");
                    return;
                }

                // Yeni işlem oluştur
                var transaction = new TransactionModel
                {
                    UserId = wallet.UserId,
                    WalletAddress = wallet.Address,
                    SenderAddress = tx.From ?? "unknown",
                    TokenId = 1, // ETH
                    Amount = amount,
                    Commission = 0,
                    Type = "deposit",
                    Status = "completed",
                    TxHash = tx.TransactionHash,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Transactions.Add(transaction);
                await _dbContext.SaveChangesAsync();

                Console.WriteLine($"[SUCCESS] {transaction.Amount} ETH başarıyla kaydedildi. Tx: {transaction.TxHash}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] İşlem kaydetme hatası: {ex.Message}");
            }
        }
    }
}