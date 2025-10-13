using GlobalOrbitra.Db;
using GlobalOrbitra.Models.DTO.WalletDTO;
using GlobalOrbitra.Models.UserModel;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.Json;

namespace GlobalOrbitra.Services.WalletService.WalletListenerService
{
    public class TronWalletListenerService
    {
        private readonly HttpClient _httpClient;
        private readonly AppDbContext _dbContext;
        private readonly string _apiTronMainUrl = "https://trongrid.io";
        private readonly string _apiTronNileTestUrl = "https://nile.trongrid.io";
        private readonly string _apiTronShastaTestUrl = "https://shasta.trongrid.io";

        public TronWalletListenerService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _httpClient = new HttpClient();
        }
        // TRX bakiyesini al
        public async Task<decimal> GetTrxBalanceAsync(string address)
        {
            var url = $"{_apiTronNileTestUrl}/v1/accounts/{address}";
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);

            if (!response.TryGetProperty("data", out var data) || data.GetArrayLength() == 0)
            {
                return 0;
            }
            var TRXBalance = data[0].GetProperty("balance").GetInt64();
            return TRXBalance / 1_000_000m;
        }

        public async Task CheckIncomingForUserAsync(UserWalletModel userWallet)
        {
            var txs = await GetIncomingTransactionsAsync(userWallet.Address);

            foreach (var tx in txs)
            {
                try
                {
                    // Daha önce DB'ye kaydedilmiş mi?
                    bool exists = _dbContext.Transactions.Any(t => t.TxHash == tx.TxHash);
                    if (exists)
                    {
                        Console.WriteLine($"[TRON Listener] İşlem zaten kayıtlı: {tx.TxHash}");
                        continue;
                    }

                    // Token'ı bul (symbol ve chain'e göre)
                    var token = await _dbContext.TokenModels
                        .FirstOrDefaultAsync(t => t.Symbol == tx.TokenSymbol && t.ChainId == 5);

                    if (token == null)
                    {
                        Console.WriteLine($"[TRON Listener] Token bulunamadı: {tx.TokenSymbol}");
                        continue;
                    }

                    var transaction = new TransactionModel
                    {
                        UserId = userWallet.UserId,
                        WalletAddress = tx.To,
                        SenderAddress = tx.From,
                        Amount = tx.Amount,
                        Type = "deposit",
                        Status = "completed",
                        CreatedAt = tx.Timestamp,
                        Commission = 0,
                        TokenId = token.Id, // Bulunan token'ın ID'si
                        TxHash = tx.TxHash
                        // TokenContract property'si kullanılmıyor
                    };

                    _dbContext.Transactions.Add(transaction);
                    Console.WriteLine($"[TRON Listener] {tx.TokenSymbol} işlemi kaydedildi: {tx.Amount} {tx.TokenSymbol}, From: {tx.From}, TxHash: {tx.TxHash}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TRON Listener] İşlem kaydetme hatası: {ex.Message}");
                }
            }

            await _dbContext.SaveChangesAsync();
        }

        // Unique constraint hatasını kontrol et
        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
                   (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }

        public async Task<List<BlockchainTransactionDto>> GetIncomingTransactionsAsync(string address, bool useMainnet = false, int limit = 50)
        {
            var transactions = new List<BlockchainTransactionDto>();

            // Hem normal transactions hem de TRC20 transactions'ları al
            var normalTxs = await GetNormalTransactionsAsync(address, limit);
            var trc20Txs = await GetTRC20TransactionsAsync(address, limit);

            transactions.AddRange(normalTxs);
            transactions.AddRange((IEnumerable<BlockchainTransactionDto>)trc20Txs);

            return transactions;
        }

        // Normal TRX transferleri
        private async Task<List<BlockchainTransactionDto>> GetNormalTransactionsAsync(string address, int limit)
        {
            var url = $"{_apiTronNileTestUrl}/v1/accounts/{address}/transactions?limit={limit}&only_confirmed=true";
            var transactions = new List<BlockchainTransactionDto>();

            try
            {
                var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);
                if (!response.TryGetProperty("data", out var txList))
                    return transactions;

                foreach (var tx in txList.EnumerateArray())
                {
                    try
                    {
                        var raw = tx.GetProperty("raw_data");
                        var contract = raw.GetProperty("contract")[0];
                        var contractType = contract.GetProperty("type").GetString();
                        var parameter = contract.GetProperty("parameter").GetProperty("value");

                        // Sadece TransferContract işlemleri (TRX transferleri)
                        if (contractType != "TransferContract")
                            continue;

                        if (!parameter.TryGetProperty("to_address", out var toProp)) continue;

                        var toHex = toProp.GetString() ?? "";
                        var toBase58 = HexToBase58(toHex);

                        // Sadece bu cüzdana gelen işlemler
                        if (!string.Equals(toBase58, address, StringComparison.OrdinalIgnoreCase))
                            continue;

                        var fromHex = parameter.GetProperty("owner_address").GetString() ?? "";
                        var fromBase58 = HexToBase58(fromHex);
                        var amount = parameter.GetProperty("amount").GetDecimal() / 1_000_000m;

                        // Timestamp çekimi
                        long timestamp = raw.GetProperty("timestamp").GetInt64();
                        DateTime transactionDate = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;

                        transactions.Add(new BlockchainTransactionDto
                        {
                            TxHash = tx.GetProperty("txID").GetString() ?? "",
                            From = fromBase58,
                            To = toBase58,
                            Amount = amount,
                            Network = "TRON",
                            Timestamp = transactionDate,
                            TokenSymbol = "TRX"
                        });

                        Console.WriteLine($"[TRX] {amount} TRX transferi bulundu: {fromBase58} -> {toBase58}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TRON Listener] TRX TX error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TRON Listener] TRX fetch error: {ex.Message}");
            }

            return transactions;
        }

        // TRC20 Token transferleri
        private async Task<List<BlockchainTransactionDto>> GetTRC20TransactionsAsync(string address, int limit)
        {
            var url = $"{_apiTronNileTestUrl}/v1/accounts/{address}/transactions/trc20?limit={limit}&only_confirmed=true";
            var transactions = new List<BlockchainTransactionDto>();

            try
            {
                var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);
                if (!response.TryGetProperty("data", out var txList))
                    return transactions;

                Console.WriteLine($"[TRC20] {txList.GetArrayLength()} TRC20 işlemi bulundu");

                foreach (var tx in txList.EnumerateArray())
                {
                    try
                    {
                        // Property'leri al
                        var toAddress = tx.GetProperty("to").GetString();
                        var fromAddress = tx.GetProperty("from").GetString();
                        var valueStr = tx.GetProperty("value").GetString();
                        var txHash = tx.GetProperty("transaction_id").GetString();
                        var timestamp = tx.GetProperty("block_timestamp").GetInt64();

                        // Sadece bu cüzdana gelen transferler
                        if (!string.Equals(toAddress, address, StringComparison.OrdinalIgnoreCase))
                            continue;

                        // Token info'dan contract address ve symbol'ü al
                        var tokenInfo = tx.GetProperty("token_info");
                        var contractAddress = tokenInfo.GetProperty("address").GetString();
                        var tokenSymbol = tokenInfo.GetProperty("symbol").GetString();
                        var tokenDecimals = tokenInfo.GetProperty("decimals").GetInt32();

                        // Token'ı veritabanından bul
                        var token = await _dbContext.TokenModels
                            .FirstOrDefaultAsync(t => t.ContractAddress == contractAddress && t.ChainId == 5);

                        if (token == null)
                        {
                            Console.WriteLine($"[TRC20] Bilinmeyen token contract: {contractAddress}");
                            continue;
                        }

                        // Amount hesapla
                        if (decimal.TryParse(valueStr, out var rawAmount))
                        {
                            var amount = rawAmount / (decimal)Math.Pow(10, tokenDecimals);
                            var transactionDate = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;

                            transactions.Add(new BlockchainTransactionDto
                            {
                                TxHash = txHash,
                                From = fromAddress,
                                To = toAddress,
                                Amount = amount,
                                Network = "TRON",
                                Timestamp = transactionDate,
                                TokenSymbol = tokenSymbol
                            });

                            Console.WriteLine($"[TRC20] ✅ {amount} {tokenSymbol} transferi: {fromAddress} -> {toAddress}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[TRC20] Hata: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TRC20] Fetch hatası: {ex.Message}");
            }

            return transactions;
        }

        // MANUEL BASE58 IMPLEMENTATION
        private string HexToBase58(string hex)
        {
            if (string.IsNullOrEmpty(hex) || !hex.StartsWith("41") || hex.Length != 42)
                return hex;

            try
            {
                byte[] hexBytes = new byte[21];
                for (int i = 0; i < 21; i++)
                {
                    hexBytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }
                return Base58CheckEncode(hexBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TRON Listener] Adres dönüşüm hatası: {ex.Message}");
                return hex;
            }
        }

        private string Base58CheckEncode(byte[] data)
        {
            byte[] hash1 = SHA256.HashData(data);
            byte[] hash2 = SHA256.HashData(hash1);
            byte[] checksum = new byte[4];
            Array.Copy(hash2, checksum, 4);

            byte[] bytesWithChecksum = new byte[data.Length + 4];
            Array.Copy(data, 0, bytesWithChecksum, 0, data.Length);
            Array.Copy(checksum, 0, bytesWithChecksum, data.Length, 4);

            return Base58Encode(bytesWithChecksum);
        }

        private string Base58Encode(byte[] data)
        {
            const string ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
            System.Numerics.BigInteger intData = 0;
            for (int i = 0; i < data.Length; i++)
            {
                intData = intData * 256 + data[i];
            }

            string result = "";
            while (intData > 0)
            {
                int remainder = (int)(intData % 58);
                intData /= 58;
                result = ALPHABET[remainder] + result;
            }

            for (int i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }

            return result;
        }

    }
}