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



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1️⃣ Chain seed
            modelBuilder.Entity<ChainModel>().HasData(
                new ChainModel { Id = 1, Name = "Ethereum", Symbol = "ETH", ChainType = "EVM", RpcUrl = "https://mainnet.infura.io/v3/YOUR_PROJECT_ID", IsActive = true },
                new ChainModel { Id = 2, Name = "Tron", Symbol = "TRX", ChainType = "TRON", RpcUrl = "https://api.trongrid.io", IsActive = true },
                new ChainModel { Id = 3, Name = "Binance Smart Chain", Symbol = "BSC", ChainType = "EVM", RpcUrl = "https://bsc-dataseed.binance.org", IsActive = true },
                new ChainModel { Id = 4, Name = "Solana", Symbol = "SOL", ChainType = "Solana", RpcUrl = "https://api.mainnet-beta.solana.com", IsActive = true }
            );

            // 2️⃣ Token seed
            modelBuilder.Entity<TokenModel>().HasData(
     new TokenModel { Id = 1, Name = "ETH", Symbol = "ETH", ContractAddress = "0x0000000000000000000000000000000000000000", Decimal = 18, IsToken = false, ChainId = 1, IsActive = true },
     new TokenModel { Id = 2, Name = "TRX", Symbol = "TRX", ContractAddress = "TRX_NATIVE", Decimal = 6, IsToken = false, ChainId = 2, IsActive = true },
     new TokenModel { Id = 3, Name = "BSC", Symbol = "BSC", ContractAddress = "BSC_NATIVE", Decimal = 18, IsToken = false, ChainId = 3, IsActive = true },
     new TokenModel { Id = 4, Name = "SOL", Symbol = "SOL", ContractAddress = "SOL_NATIVE", Decimal = 9, IsToken = false, ChainId = 4, IsActive = true }
 );

        }
    }
}
