using Nethereum.Signer;

namespace GlobalOrbitra.Services.WalletService
{
    public class EthWalletService
    {
        public async Task<string> EthCreateWallet()
        {
            var key = EthECKey.GenerateKey();
            var privateKey = key.GetPrivateKey();
            var address = key.GetPublicAddress();
            return $"ETH CREATED WALLET {address}";
        }
    }
}
