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
                // Daha önce DB'ye kaydedilmiş mi? (HER ZAMAN KONTROL ET)
                bool exists = _dbContext.Transactions.Any(t => t.TxHash == tx.TxHash);
                if (exists)
                {
                    //Console.WriteLine($"[TRON Listener] İşlem zaten kayıtlı: {tx.TxHash}");
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
                    TokenId = userWallet.TokenId,
                    TxHash = tx.TxHash // BU SATIR ÇOK ÖNEMLİ!
                };

                _dbContext.Transactions.Add(transaction);
                Console.WriteLine($"[TRON Listener] Wallet {userWallet.Address} için işlem kaydedildi: TxHash={tx.TxHash}, Amount={tx.Amount} TRX, From={tx.From}");
            }

            await _dbContext.SaveChangesAsync();
        }

        // Unique constraint hatasını kontrol et
        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx &&
                   (sqlEx.Number == 2601 || sqlEx.Number == 2627);
        }

        public async Task<List<BlockchainTransactionDTO>> GetIncomingTransactionsAsync(string address, bool useMainnet = false, int limit = 50)
        {
            var url = $"{_apiTronNileTestUrl}/v1/accounts/{address}/transactions?limit=20&sort=-timestamp";
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(url);

            var transactions = new List<BlockchainTransactionDTO>();
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

                    var toHex = toProp.GetString() ?? "";

                    // YENİ METODU KULLAN
                    var toBase58 = HexToBase58(toHex);

                    // Adres eşleşme kontrolü
                    if (!string.Equals(toBase58, address, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"[TRON Listener] TX to {toBase58} Wallet {address} ile eşleşmedi, atlandı.");
                        continue;
                    }

                    var fromHex = parameter.GetProperty("owner_address").GetString() ?? "";
                    var fromBase58 = HexToBase58(fromHex);
                    var amount = parameter.GetProperty("amount").GetDecimal() / 1_000_000m;

                    transactions.Add(new BlockchainTransactionDTO
                    {
                        TxHash = tx.GetProperty("txID").GetString() ?? "",
                        From = fromBase58,
                        To = toBase58,
                        Amount = amount,
                        Network = "TRON"
                    });

                    Console.WriteLine($"[TRON Listener] Wallet {address} için işlem bulundu: TxHash={tx.GetProperty("txID").GetString()}, Amount={amount} TRX, From={fromBase58}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[TRON Listener] Hatalı tx atlandı: {ex.Message}");
                }
            }

            return transactions;
        }

        // YENİ METOD - MANUEL BASE58 IMPLEMENTATION
        private string HexToBase58(string hex)
        {
            if (string.IsNullOrEmpty(hex) || !hex.StartsWith("41") || hex.Length != 42)
                return hex;

            try
            {
                // Hex string'i byte array'e çevir
                byte[] hexBytes = new byte[21]; // 20 byte address + 1 byte prefix
                for (int i = 0; i < 21; i++)
                {
                    hexBytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                }

                // Base58Check encoding uygula
                return Base58CheckEncode(hexBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TRON Listener] Adres dönüşüm hatası: {ex.Message}");
                return hex;
            }
        }

        /// <summary>
        /// Base58Check encoding implementation for Tron
        /// </summary>
        private string Base58CheckEncode(byte[] data)
        {
            // Add checksum: SHA256(SHA256(data))
            byte[] hash1 = SHA256.HashData(data);
            byte[] hash2 = SHA256.HashData(hash1);

            byte[] checksum = new byte[4];
            Array.Copy(hash2, checksum, 4);

            // Data + checksum birleştir
            byte[] bytesWithChecksum = new byte[data.Length + 4];
            Array.Copy(data, 0, bytesWithChecksum, 0, data.Length);
            Array.Copy(checksum, 0, bytesWithChecksum, data.Length, 4);

            return Base58Encode(bytesWithChecksum);
        }

        /// <summary>
        /// Base58 encoding implementation
        /// </summary>
        private string Base58Encode(byte[] data)
        {
            const string ALPHABET = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

            // Convert byte array to BigInteger
            System.Numerics.BigInteger intData = 0;
            for (int i = 0; i < data.Length; i++)
            {
                intData = intData * 256 + data[i];
            }

            // Encode BigInteger to Base58 string
            string result = "";
            while (intData > 0)
            {
                int remainder = (int)(intData % 58);
                intData /= 58;
                result = ALPHABET[remainder] + result;
            }

            // Add '1' for each leading 0 byte
            for (int i = 0; i < data.Length && data[i] == 0; i++)
            {
                result = '1' + result;
            }

            return result;
        }

    }
}