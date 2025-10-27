using UnifiedChainWallet.Models;
using UnifiedChainWallet.Services.WalletService;

namespace GlobalOrbitra.Services.WalletService
{
    public class WalletService
    {
        public WalletModel TronWallet { get; private set; }
        public WalletModel EthWallet { get; private set; }
        public WalletModel BscWallet { get; private set; }
        public WalletModel BttcWallet { get; private set; }
        public WalletModel SolWallet { get; private set; }
        public WalletService()
        {
            TronWalletService tronWalletService = new TronWalletService();
            TronWallet = tronWalletService.TronCreateWallet();

            EvmWalletService evmWalletService = new EvmWalletService();
            EthWallet = evmWalletService.EvmCreateWallet("ETH");
            BscWallet = evmWalletService.EvmCreateWallet("BSC");
            BttcWallet = evmWalletService.EvmCreateWallet("BTTC");
            SolWallet = evmWalletService.EvmCreateWallet("SOL");

            SolWalletService solWalletService = new SolWalletService();
            SolWallet = solWalletService.SolCreateWallet();
        }
    }
}
