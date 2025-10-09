using GlobalOrbitra.Db;
using GlobalOrbitra.Models.UserModel;
using Nethereum.JsonRpc.Client;
using System.Net.Http.Json;
using System.Text.Json;

namespace GlobalOrbitra.Services.WalletService.WalletListenerService
{
    public class TronListenerService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly string _apiTronMainUrl = "https://api.trongrid.io"; // Tron ana ağı için API URL'si
        private readonly string _apiTronNileTestUrl = "https://api.nile.trongrid.io"; // Tron  nile test ağı için API URL'si
        private readonly string _apiTronShastaTestUrl = "https://api.shasta.trongrid.io"; // Tron shasta test ağı için API URL'si


        public TronListenerService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _httpClient = new HttpClient();
        }


        // Tron Ağında TRX Cinsinden Cüzdan Bakiyesini Getirir

        public async Task<decimal> GetTrxBalanceAsync(string address)
        {
            var url = $"{_apiTronNileTestUrl}/v1/accounts/{address}";
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);

            if (!response.TryGetProperty("data", out var data) || data.GetArrayLength() == 0) // Json yanıtı kontrol ediyor.
            {
                return 0;
            }
            var TRXBalance = data[0].GetProperty("balance").GetInt64();
            return TRXBalance / 1_000_000m; // TRX 6 ondalık basamağa sahiptir.
        }


        // Belirli bir kullanıcı cüzdanını dinler. Yeni transfer gelirse Transaction tablosuna ekler.
        public async Task CheckIncomingForUserAsync(UserWalletModel userWallet)
        {
            var txs = await GetIncomingTransactionsAsync(userWallet.Address);

            foreach (var tx in txs)
            {
                // Daha önce DB’ye kaydedilmiş mi?
                bool exists = _dbContext.Transactions.Any(t => t.TxHash == tx.TxHash);
                if (exists) continue;

                var transaction = new TransactionModel
                {
                    UserId = userWallet.UserId,
                    WalletAddress = tx.To,
                    Amount = tx.Amount,
                    Type = "deposit",
                    Status = "completed",
                    CreatedAt = DateTime.UtcNow,
                    Commission = 0, // TRON’da genelde fee yok
                    TokenId = userWallet.TokenId // TRX token için ilgili TokenId
                };

                _dbContext.Transactions.Add(transaction);
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<BlockchainTransaction>> GetIncomingTransactionsAsync(string address, bool useMainnet = false, int limit = 50)
        {
            var url = $"{_apiTronNileTestUrl}/v1/accounts/{address}/transactions?limit=20&sort=-timestamp";
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);

            var transactions = new List<BlockchainTransaction>();
            if (!response.TryGetProperty("data", out var txList))
                return transactions;

            foreach (var tx in txList.EnumerateArray())
            {
                try
                {
                    var raw = tx.GetProperty("raw_data");
                    var contract = raw.GetProperty("contract")[0];
                    var parameter = contract.GetProperty("parameter").GetProperty("value");

                    if (!parameter.TryGetProperty("to_address", out var toProp)) continue;

                    var to = Base58Check(toProp.GetString() ?? "");
                    if (!string.Equals(to, address, StringComparison.OrdinalIgnoreCase)) continue;

                    var from = Base58Check(parameter.GetProperty("owner_address").GetString() ?? "");
                    var amount = parameter.GetProperty("amount").GetDecimal() / 1_000_000m;

                    transactions.Add(new BlockchainTransaction
                    {
                        TxHash = tx.GetProperty("txID").GetString() ?? "",
                        From = from,
                        To = to,
                        Amount = amount,
                        Network = "TRON"
                    });
                }
                catch
                {
                    // Hatalı tx varsa atla
                }
            }

            return transactions;
        }

        private string Base58Check(string hex)
        {
            if (hex.StartsWith("41"))
                return "T" + hex.Substring(2, 33); // Basit TRON dönüşümü
            return hex;
        }

        public class BlockchainTransaction
        {
            public string TxHash { get; set; } = null!;
            public string From { get; set; } = null!;
            public string To { get; set; } = null!;
            public decimal Amount { get; set; }
            public string Network { get; set; } = null!;
        }
    }
     
}
