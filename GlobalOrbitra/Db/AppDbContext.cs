using GlobalOrbitra.Models.MailModel;
using GlobalOrbitra.Models.UserModel;
using Microsoft.EntityFrameworkCore;

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

            // ✅ Transaction hash için unique index eklendi
            modelBuilder.Entity<TransactionModel>()
                .HasIndex(t => t.TxHash)
                .IsUnique();

            // 🌐 CHAINS
            modelBuilder.Entity<ChainModel>().HasData(
                // Mainnets
                new ChainModel { Id = 1, Name = "Ethereum", Symbol = "ETH", ChainType = "EVM", RpcUrl = "https://mainnet.infura.io/v3/YOUR_PROJECT_ID", IsActive = true },
                new ChainModel { Id = 2, Name = "Tron", Symbol = "TRX", ChainType = "TRON", RpcUrl = "https://api.trongrid.io", IsActive = true },
                new ChainModel { Id = 3, Name = "Binance Smart Chain", Symbol = "BSC", ChainType = "EVM", RpcUrl = "https://bsc-dataseed.binance.org", IsActive = true },
                new ChainModel { Id = 4, Name = "Solana", Symbol = "SOL", ChainType = "Solana", RpcUrl = "https://api.mainnet-beta.solana.com", IsActive = true },
                new ChainModel { Id = 5, Name = "BitTorrent Chain", Symbol = "BTTC", ChainType = "EVM", RpcUrl = "https://api.mainnet-beta.com", IsActive = true },

                //---------------------TEST AĞLARI------------------------------
                new ChainModel { Id = 6, Name = "Tron Nile Testnet", Symbol = "TRX", ChainType = "TRON", RpcUrl = "https://nile.trongrid.io", IsActive = true },
                new ChainModel { Id = 7, Name = "Ethereum Sepolia Testnet", Symbol = "ETH", ChainType = "EVM", RpcUrl = "https://sepolia.infura.io/v3/YOUR_PROJECT_ID", IsActive = true },
                new ChainModel { Id = 8, Name = "BitTorrent Chain Testnet", Symbol = "BTTC", ChainType = "EVM", RpcUrl = "https://api.testnet.bittorrentchain.io", IsActive = true },
                new ChainModel { Id = 9, Name = "Binance Smart Chain Testnet", Symbol = "BSC", ChainType = "EVM", RpcUrl = "https://api.testnet.bittorrentchain.io", IsActive = true }
            );

            // 💰 TOKENS
            modelBuilder.Entity<TokenModel>().HasData(
                // === MAINNET TOKENS ===
                new TokenModel { Id = 1, Name = "ETH", Symbol = "ETH", ContractAddress = "ETH_NATIVE", Decimal = 18, IsToken = false, ChainId = 1, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/ethereum-eth-logo.png" },
                new TokenModel { Id = 2, Name = "TRX", Symbol = "TRX", ContractAddress = "TRX_NATIVE", Decimal = 6, IsToken = false, ChainId = 2, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/tron-trx-logo.png" },
                new TokenModel { Id = 3, Name = "BSC", Symbol = "BSC", ContractAddress = "BSC_NATIVE", Decimal = 18, IsToken = false, ChainId = 3, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/binance-smart-chain-bsc-logo.png" },
                new TokenModel { Id = 4, Name = "SOL", Symbol = "SOL", ContractAddress = "SOL_NATIVE", Decimal = 9, IsToken = false, ChainId = 4, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/solana-sol-logo.svg" },
                new TokenModel { Id = 5, Name = "BTT", Symbol = "BTT", ContractAddress = "BTT_NATIVE", Decimal = 18, IsToken = true, ChainId = 5, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/bittorrent-btt-logo.png" },


                //---------------------TEST TOKENLERİ------------------------------

                // ✅ TRON NILE TESTNET TOKENS
                new TokenModel { Id = 6, Name = "TRX (Testnet)", Symbol = "TRX", ContractAddress = "TRX_NATIVE", Decimal = 6, IsToken = false, ChainId = 5, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/tron-trx-logo.png" },
                new TokenModel { Id = 7, Name = "Tether USDT (Nile)", Symbol = "USDT", ContractAddress = "TXYZopYRdj2D9XRtbG411XZZ3kM5VkAeBf", Decimal = 6, IsToken = true, ChainId = 5, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/tether-usdt-logo.png" },
                new TokenModel { Id = 8, Name = "USD Coin (Nile)", Symbol = "USDC", ContractAddress = "TEMVynQpntMqkPxP6wXTW2K7e4sM3cRmWz", Decimal = 6, IsToken = true, ChainId = 5, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/usd-coin-usdc-logo.png" },
                new TokenModel { Id = 9, Name = "BTT (Nile)", Symbol = "BTT", ContractAddress = "TVSvjZdyDSNocHm7dP3jvCmMNsCnMTPa5W", Decimal = 18, IsToken = true, ChainId = 5, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/bittorrent-btt-logo.png" },
                new TokenModel { Id = 10, Name = "USDD Token (Nile)", Symbol = "USDD", ContractAddress = "TFT7sNiNDGZcqL7z7dwXUPpxrx1Ewk8iGL", Decimal = 18, IsToken = true, ChainId = 5, IsActive = true, LogoUrl = "https://logo.svgcdn.com/token-branded/usdd.png" },

                //-------------------------------------------------

                // ✅ ETH SEPOLIA TESTNET TOKENS EKLENDİ
                new TokenModel { Id = 11, Name = "ETH (Sepolia)", Symbol = "ETH", ContractAddress = "0x0000000000000000000000000000000000000000", Decimal = 18, IsToken = false, ChainId = 7, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/ethereum-eth-logo.png" },
                new TokenModel { Id = 12, Name = "USDT (Sepolia)", Symbol = "USDT", ContractAddress = "0xaA8E23Fb1079EA71e0a56F48a2aA51851D8433D0", Decimal = 6, IsToken = true, ChainId = 7, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/tether-usdt-logo.png" },
                new TokenModel { Id = 13, Name = "USDC (Sepolia)", Symbol = "USDC", ContractAddress = "0x1c7D4B196Cb0C7B01d743Fbc6116a902379C7238", Decimal = 6, IsToken = true, ChainId = 7, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/usd-coin-usdc-logo.png" },
                //-------------------------------------------------

                // ✅ BTT TESTNET TOKENS EKLENDİ
                new TokenModel { Id = 14, Name = "BTT Testnet", Symbol = "BTT", ContractAddress = "BTT_NATIVE", Decimal = 6, IsToken = true, ChainId = 8, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/bittorrent-btt-logo.png" },

                // ✅ BSC TESTNET TOKENS EKLENDİ
                new TokenModel { Id = 15, Name = "BSC Testnet", Symbol = "BSC", ContractAddress = "BSC_NATIVE", Decimal = 18, IsToken = false, ChainId = 9, IsActive = true, LogoUrl = "https://cryptologos.cc/logos/binance-smart-chain-bsc-logo.png" }
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

