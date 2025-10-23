using GlobalOrbitra.Db;
using GlobalOrbitra.Models.UserModel;
using Microsoft.EntityFrameworkCore;
using Nethereum.Web3;
using System.Numerics;

namespace GlobalOrbitra.Services.WalletService.WalletListenerService
{
    public class EthWalletListenerService
    {
        private readonly AppDbContext _dbContext;
        private readonly Web3 _web3;

        // Sepolia RPC URL sabit
        private const string SepoliaRpcUrl = "https://sepolia.infura.io/v3/";
        public EthWalletListenerService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
            _web3 = new Web3(SepoliaRpcUrl);
        }
        // Kullanıcının ETH ve ERC20 bakiyelerini kontrol et ve DB'ye kaydet
        public async Task CheckIncomingForUserAsync(UserWalletModel userWallet)
        {
            // ETH Bakiyesi
            var weiBalance = await _web3.Eth.GetBalance.SendRequestAsync(userWallet.Address);
            var ethBalance = Web3.Convert.FromWei(weiBalance);

            if (ethBalance > 0)
            {
                var ethToken = await _dbContext.TokenModels
                    .FirstOrDefaultAsync(t => t.Symbol == "ETH" && t.Chain.ChainType == "EVM");

                if (ethToken != null)
                {
                    await AddTransactionIfNotExists(userWallet, ethBalance, ethToken, "ETH");
                    Console.WriteLine($"[ETH] {ethBalance} ETH bulundu: {userWallet.Address}");
                }
            }

            // ERC20 Token bakiyeleri
            var tokens = await _dbContext.TokenModels
                .Include(t => t.Chain)
                .Where(t => t.Chain.ChainType == "EVM" && t.IsToken && t.IsActive)
                .ToListAsync();

            foreach (var token in tokens)
            {
                decimal balance = await GetERC20BalanceAsync(token.ContractAddress, userWallet.Address, (int)token.Decimal);
                if (balance > 0)
                {
                    await AddTransactionIfNotExists(userWallet, balance, token, token.Symbol);
                    Console.WriteLine($"[ERC20] {balance} {token.Symbol} bulundu: {userWallet.Address}");
                }
            }
        }

        // ERC20 bakiyesi çek
        private async Task<decimal> GetERC20BalanceAsync(string contractAddress, string walletAddress, int decimals)
        {
            var contract = _web3.Eth.GetContract(ERC20ABI, contractAddress);
            var balanceFunction = contract.GetFunction("balanceOf");
            BigInteger balance = await balanceFunction.CallAsync<BigInteger>(walletAddress);
            return (decimal)balance / (decimal)Math.Pow(10, decimals);
        }

        // Transaction ekle (varsa DB kontrolü)
        private async Task AddTransactionIfNotExists(UserWalletModel userWallet, decimal amount, TokenModel token, string symbol)
        {
            bool exists = await _dbContext.Transactions.AnyAsync(t =>
                t.WalletAddress == userWallet.Address && t.TokenId == token.Id && t.Type == "deposit");

            if (!exists)
            {
                var transaction = new TransactionModel
                {
                    UserId = userWallet.UserId,
                    WalletAddress = userWallet.Address,
                    SenderAddress = userWallet.Address, // ETH/Token geldiği adresi manuel olarak koyabiliriz
                    Amount = amount,
                    Type = "deposit",
                    Status = "completed",
                    TokenId = token.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _dbContext.Transactions.Add(transaction);
                await _dbContext.SaveChangesAsync();
            }
        }

        private const string ERC20ABI = @"[
            {""constant"":true,""inputs"":[{""name"":""_owner"",""type"":""address""}],
            ""name"":""balanceOf"",""outputs"":[{""name"":""balance"",""type"":""uint256""}],
            ""type"":""function""}
        ]";

    }
}
