using GlobalOrbitra.Models.MailModel;
using GlobalOrbitra.Models.UserModel;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace GlobalOrbitra.Db
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<UserModel> UserModels { get; set; } = null!;
        public DbSet<UserWalletModel> UserWalletModels { get; set; } = null!;
        public DbSet<TokenModel> TokenModels { get; set; } = null!;
        public DbSet<TransactionModel> AssetTransactionModels { get; set; } = null!;
        public DbSet<TransactionModel> Transactions { get; set; } = null!;
        public DbSet<OtpCodeModel> OtpCodes { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Decimal precision
          //  modelBuilder.Entity<UserWalletModel>().Property(u => u.Balance).HasPrecision(18, 6);

            // 🌐 CHAINS
            modelBuilder.Entity<ChainModel>().HasData(
                // Mainnets
                new ChainModel { Id = 1, Name = "Ethereum", Symbol = "ETH", ChainType = "EVM", RpcUrl = "https://mainnet.infura.io/v3/YOUR_PROJECT_ID", IsActive = true },
                new ChainModel { Id = 2, Name = "Tron", Symbol = "TRX", ChainType = "TRON", RpcUrl = "https://api.trongrid.io", IsActive = true },
                new ChainModel { Id = 3, Name = "Binance Smart Chain", Symbol = "BSC", ChainType = "EVM", RpcUrl = "https://bsc-dataseed.binance.org", IsActive = true },
                new ChainModel { Id = 4, Name = "Solana", Symbol = "SOL", ChainType = "Solana", RpcUrl = "https://api.mainnet-beta.solana.com", IsActive = true },

                // ✅ Tron Testnet (Nile)
                new ChainModel { Id = 5, Name = "Tron Nile Testnet", Symbol = "TRX", ChainType = "TRON", RpcUrl = "https://nile.trongrid.io", IsActive = true }
            );

            // 💰 TOKENS
            modelBuilder.Entity<TokenModel>().HasData(
                // === MAINNET TOKENS ===
                new TokenModel { Id = 1, Name = "ETH", Symbol = "ETH", ContractAddress = "0x0000000000000000000000000000000000000000", Decimal = 18, IsToken = false, ChainId = 1, IsActive = true },
                new TokenModel { Id = 2, Name = "TRX", Symbol = "TRX", ContractAddress = "TRX_NATIVE", Decimal = 6, IsToken = false, ChainId = 2, IsActive = true },
                new TokenModel { Id = 3, Name = "BSC", Symbol = "BSC", ContractAddress = "BSC_NATIVE", Decimal = 18, IsToken = false, ChainId = 3, IsActive = true },
                new TokenModel { Id = 4, Name = "SOL", Symbol = "SOL", ContractAddress = "SOL_NATIVE", Decimal = 9, IsToken = false, ChainId = 4, IsActive = true },

                // === 🧪 TRON NILE TESTNET TOKENS ===
                new TokenModel { Id = 5, Name = "TRX (Testnet)", Symbol = "TRX", ContractAddress = "TRX_NATIVE", Decimal = 6, IsToken = false, ChainId = 5, IsActive = true },
                new TokenModel { Id = 6, Name = "Tether USDT (Nile)", Symbol = "USDT", ContractAddress = "TXYZopYRdj2D9XRtbG411XZZ3kM5VkAeBf", Decimal = 6, IsToken = true, ChainId = 5, IsActive = true },
                new TokenModel { Id = 7, Name = "USD Coin (Nile)", Symbol = "USDC", ContractAddress = "TEMVynQpntMqkPxP6wXTW2K7e4sM3cRmWz", Decimal = 6, IsToken = true, ChainId = 5, IsActive = true },
                new TokenModel { Id = 8, Name = "BTT (Nile)", Symbol = "BTT", ContractAddress = "TVSvjZdyDSNocHm7dP3jvCmMNsCnMTPa5W", Decimal = 18, IsToken = true, ChainId = 5, IsActive = true },
                new TokenModel { Id = 9, Name = "USDD Token (Nile)", Symbol = "USDD", ContractAddress = "TFT7sNiNDGZcqL7z7dwXUPpxrx1Ewk8iGL", Decimal = 18, IsToken = true, ChainId = 5, IsActive = true }
            );

            modelBuilder.Entity<TokenModel>()
                .Property(t => t.Decimal)
                .HasPrecision(38, 18); // blockchain token değerleri için yüksek precision

            modelBuilder.Entity<TransactionModel>()
                .Property(t => t.Amount)
                .HasPrecision(38, 18);

            modelBuilder.Entity<TransactionModel>()
                .Property(t => t.Commission)
                .HasPrecision(38, 18);
        }
    }
}
